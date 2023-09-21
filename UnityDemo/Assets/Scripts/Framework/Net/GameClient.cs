
using ClientProto;
using Geek.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Base.Net
{
    public class GameClient
    {
        private const float DISPATCH_MAX_TIME = 0.06f;  //每一帧最大的派发事件时间，超过这个时间则停止派发，等到下一帧再派发 
        public static GameClient Singleton = new GameClient();
        private NetChannel channel { get; set; }
        ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();
        public int Port { private set; get; }
        public string Host { private set; get; }

        public void Send(Message msg)
        {
            channel?.Write(msg);
        }

        public async Task<bool> Connect(string host, int port, int timeOut = 5000)
        {
            Host = host;
            Port = port;
            try
            {
                ClearAllMsg();
                var ipType = AddressFamily.InterNetwork;
                (ipType, host) = NetUtils.GetIPv6Address(host, port);

                var socket = new TcpClient(ipType);
                try
                {
                    await socket.ConnectAsync(host, port);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    return false;
                }

                if (!socket.Connected)
                {
                    return false;
                }

                Debug.Log($"connected success....");
                OnConnected();
                channel = new NetChannel(socket, OnRevice, OnDisConnected);
                _ = channel.StartAsync();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return false;
            }
        }

        private void OnConnected()
        {
        }

        public void OnDisConnected()
        {
            msgQueue.Enqueue(new NetDisConnectMessage());
        }

        public void OnRevice(Message msg)
        {
            msgQueue.Enqueue(msg);
        }

        public void Close()
        {
            channel?.Close();
            channel = null;
            ClearAllMsg();
        }

        public void ClearAllMsg()
        {
            msgQueue = new ConcurrentQueue<Message>();
        }

        public void Update(EventDispatcher evt, float maxTime = DISPATCH_MAX_TIME)
        {
            float curTime = UnityEngine.Time.realtimeSinceStartup;
            float endTime = curTime + maxTime;
            while (curTime < endTime)
            {
                if (msgQueue.IsEmpty)
                    return;

                if (!msgQueue.TryDequeue(out var msg))
                    break;

                if (msg == null)
                    return;

#if UNITY_EDITOR 
                var msgName = msg != null ? msg.GetType().FullName : "";
                Debug.Log($"开始处理网络事件 {msg.MsgId}  {msgName}");
#endif

                try
                {
                    evt.dispatchEvent(msg.MsgId, msg);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }

                curTime = UnityEngine.Time.realtimeSinceStartup;
                if (!ignoreCodeList.Contains(msg.MsgId))
                    ResCode++;
            }
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