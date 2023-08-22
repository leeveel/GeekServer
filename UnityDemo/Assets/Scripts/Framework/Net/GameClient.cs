
using Geek.Client;
using Geek.Server.Proto;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Base.Net
{
    public class GateList
    {
        public List<string> serverIps = new List<string>();
        public List<int> ports = new List<int>();
    }
    public class GameClient
    {
        private const float DISPATCH_MAX_TIME = 0.06f;  //每一帧最大的派发事件时间，超过这个时间则停止派发，等到下一帧再派发

        public static GameClient Singleton = new GameClient();
        private KcpChannel channel { get; set; }
        ConcurrentQueue<Message> msgQueue = new ConcurrentQueue<Message>();
        public int Port { private set; get; }
        public string Host { private set; get; }
        public void Send(Message msg)
        {
            channel?.Write(msg);
        }

        public async void Connect(GateList gateList, int serverId, int timeOut = 5000)
        {
            long netId = 0;
            KcpChannel kcpChannel = null;
            async Task Connect(int delay = 0)
            {
                if (!Application.isPlaying)
                    return;
                if (delay > 0)
                {
                    await Task.Delay(delay);
                }
                Debug.Log("选择网关...");
                var index = UnityEngine.Random.Range(0, gateList.serverIps.Count);
                var ip = gateList.serverIps[index];
                var port = gateList.ports[index];

                IKcpSocket clientSocket = null;

                var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

                //clientSocket = new KcpUdpClientSocket(serverId);
                clientSocket = new KcpTcpClientSocket(serverId);

                if (!await clientSocket.Connect(ip, port, netId))
                {
                    Debug.LogError("连接服务器失败...");
                    await Task.Delay(20);

                    //TODO:限制连接次数
                    await Connect(100);
                    return;
                }
                if (kcpChannel == null)
                {
                    netId = clientSocket.NetId;
                    kcpChannel = new KcpChannel(false, netId, serverId, serverEndPoint, (chann, data) =>
                    {
                        var package = new TempNetPackage(NetPackageFlag.MSG, chann.NetId, serverId, data);
                        clientSocket?.Send(package);
                    }, (chann, msg) =>
                    {
                        OnRevice(msg);
                    });
                    channel = kcpChannel;
                    OnConnected(NetCode.Success);
                }

                _ = clientSocket.StartRecv(kcpChannel.HandleRecv, () =>
                 {
                     _ = Connect(100);
                 }, () =>
                 {
                     Debug.LogError("服务器断开连接....");
                     OnDisConnected();
                 });
            }
            await Connect();
        }

        private void OnConnected(NetCode code)
        {
            msgQueue.Enqueue(new NetConnectMessage { Code = code });
        }

        public void OnDisConnected()
        {
            msgQueue.Enqueue(new NetDisConnectMessage { Code = NetCode.Closed });
        }

        public void OnRevice(Message msg)
        {
            msgQueue.Enqueue(msg);
        }

        public void Close()
        {
            channel?.Close();
            ClearAllMsg();
        }

        public void ClearAllMsg()
        {
            msgQueue = new ConcurrentQueue<Message>();
        }

        public void Update(EventDispatcher evt, float maxTime = DISPATCH_MAX_TIME)
        {
            channel?.Update(DateTime.UtcNow);

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
                //Debug.Log($"开始处理网络事件 {msg.MsgId}  {msg.GetType().FullName}");
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