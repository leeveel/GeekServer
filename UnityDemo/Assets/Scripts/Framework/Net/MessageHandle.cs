using System;
using System.Collections.Generic;

namespace Geek.Client
{
    public class MessageHandle
    {
        private const float DISPATCH_MAX_TIME = 0.03f;  //每一帧最大的派发事件时间，超过这个时间则停止派发，等到下一帧再派发

        private static Dictionary<int, MessageHandle> instanceMap = new Dictionary<int, MessageHandle>();
        public static MessageHandle GetInstance(int instanceId = 0)
        {
            if (!instanceMap.ContainsKey(instanceId) || instanceMap[instanceId] == null)
                instanceMap[instanceId] = new MessageHandle();
            return instanceMap[instanceId];
        }

        Actor actor = new Actor();
        public const int ConnectSucceedEvt = 101; //连接成功
        public const int DisconnectEvt = 102; //连接断开

        private readonly AsyncTCPSocket socket = new AsyncTCPSocket();
        private Queue<RMessage> msgQueue = new Queue<RMessage>(); //后台线程接受队列
        readonly Queue<RMessage> dispatchMsgQueue = new Queue<RMessage>(); //主线程派发队列
        readonly MessageCoder coder = new MessageCoder();

        public void SetThreadMsgCreator(Func<int, BaseMessage> msgCreator)
        {
            coder.MsgFactory = msgCreator;
        }

        /// <summary>
        /// 获取当前的消息
        /// </summary>
        public RMessage GetCurMsg()
        {
            return dispatchMsgQueue.Peek();
        }

        /// <summary>
        /// 连接服务器
        /// </summary>
        public void BeginConnect(string ip, int port)
        {
            NetBufferPool.Init();
            socket.Connect(ip, port, ConnectCallback, DisconnectCallback, ReceiveCallback);
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        public void Send(BaseMessage msg)
        {
            actor.SendAsync(async ()=> {
                var data = NetBufferPool.Alloc(512);
                //消息头空出来
                var len = msg.Write(data, MessageCoder.EncodeHeadLength);
                if (len > data.Length)
                {
                    NetBufferPool.Free(data);
                    data = NetBufferPool.Alloc(len);
                    len = msg.Write(data, MessageCoder.EncodeHeadLength);
                }
                coder.Encode(msg.GetMsgId(), ref data, len);
                await socket.SendMsg(data, len);
                NetBufferPool.Free(data);
            });
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void CloseSocket(bool thread = true)
        {
            socket.Close();
        }

        /// <summary>
        /// 是否处于连接状态
        /// </summary>
        public bool IsConnected()
        {
            return socket.IsConnected;
        }
        
        private void ConnectCallback(NetCode code)
        {
            actor.SendAsync(() =>
            {
                coder.Clear();
                var rMsg = new RMessage();
                rMsg.MsgId = ConnectSucceedEvt;
                rMsg.RetCode = (int)code;
                msgQueue.Enqueue(rMsg);
            });
        }

        /// <summary>
        /// 断线
        /// </summary>
        private void DisconnectCallback(NetCode code)
        {
            actor.SendAsync(() =>
            {
                var rMsg = new RMessage();
                rMsg.MsgId = DisconnectEvt;
                rMsg.RetCode = (int)code;
                msgQueue.Enqueue(rMsg);
            });
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveCallback(byte[] bytes, int len)
        {
            actor.SendAsync(()=> {
                coder.Decode(bytes, len, ref msgQueue);
                NetBufferPool.Free(bytes);
            });
        }

        /// <summary>
        /// 每帧调用
        /// </summary>
        public void Update(EventDispatcher evt, float maxTime = DISPATCH_MAX_TIME)
        {
            if (evt == null)
                return;
            
            float curTime = UnityEngine.Time.realtimeSinceStartup;
            float endTime = curTime + maxTime;
            while (curTime < endTime)
            {
                if (dispatchMsgQueue.Count < 1)
                    break;

                var msg = dispatchMsgQueue.Peek();
                if (msg == null)
                    break;

                evt.dispatchEvent(msg.MsgId, msg.RetCode);
                lock (dispatchMsgQueue)
                {
                    dispatchMsgQueue.Dequeue();
                }
                NetBufferPool.Free(msg.ByteContent);
                curTime = UnityEngine.Time.realtimeSinceStartup;
                HandMsgTime = curTime;
                if (!ignoreCodeList.Contains(msg.MsgId))
                    ResCode++;
            }

            actor.SendAsync(() => {
                while (true)
                {
                    if (msgQueue.Count < 1)
                        break;

                    var msg = msgQueue.Dequeue();
                    if (msg == null)
                        break;

                    lock(dispatchMsgQueue)
                    {
                        dispatchMsgQueue.Enqueue(msg);
                    }
                }
            });
        }

        /// <summary>上次接收消失时间</summary>
        public float HandMsgTime { get; private set; }
        /// <summary>收到的消息计数 和服务器对不上则应该断线重连</summary>
        public int ResCode { get; private set; }
        List<int> ignoreCodeList = new List<int>();
        public void ResetResCode(int code = 0)
        {
            ResCode = code;
        }

        /// <summary>
        /// 心跳等无关逻辑的消息可忽略
        /// </summary>
        public void AddIgnoreCode(int msgId)
        {
            if (!ignoreCodeList.Contains(msgId))
                ignoreCodeList.Add(msgId);
        }
    }
}