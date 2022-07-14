using Bedrock.Framework;
using Geek.Server;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Client
{

    public enum NetCode
    {
        TimeOut = 100,     //超时
        Success,               //连接成功
        Disconnect,           //断开连接
        Failed,                  //链接失败
    }

    public class GameClient 
    {
        public static GameClient Singleton = new GameClient();
        private readonly Queue<Message> msgQueue = new Queue<Message>(); 
        private ClientNetChannel Channel { get; set; }
        private UniActor receiveActor = null;
        private GameClient() { }

        public void Init()
        {
            string name = SynchronizationContext.Current.GetType().FullName;
            if (name != "UnityEngine.UnitySynchronizationContext")
                UnityEngine.Debug.LogError($"只能在UnitySynchronizationContext上下文中初始化GameClient:{name}");
            else
                UnityEngine.Debug.Log($"GameClient Init Success in {name}");
            receiveActor = new UniActor();
        }

        public Message GetCurMsg()
        {
            return msgQueue.Peek();
        }

        public void Receive(Message msg)
        {
            receiveActor.SendAsync(() =>
            {
                msgQueue.Enqueue(msg);
                GED.NED.dispatchEvent(msg.MsgId);
                msgQueue.Dequeue();
            });
        }

        public void Send(Message msg)
        {
            Channel?.WriteAsync(new NMessage(msg));
        }

        public int Port { private set; get; }
        public string Host { private set; get; }
        public const int ConnectEvt = 101; //连接事件
        public const int DisconnectEvt = 102; //连接断开

        public async Task<ClientNetChannel> Connect(string host, int port)
        {
            Host = host;
            Port = port;
            try
            {
                var client = new ClientBuilder().UseSockets().Build();
                var connection = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(Host), Port));
                UnityEngine.Debug.Log($"Connected to {connection.LocalEndPoint}");
                Channel = new ClientNetChannel(connection, new ClientLengthPrefixedProtocol());
                OnConnected(NetCode.Success);
                return Channel;
            }
            catch (Exception)
            {
                OnConnected(NetCode.Failed);
                throw;
            }
        }

        private void OnConnected(NetCode code)
        {
            receiveActor.SendAsync(() =>
            {
                GED.NED.dispatchEvent(ConnectEvt, code);
            });
        }

        public void OnDisConnected()
        {
            receiveActor.SendAsync(() =>
            {
                GED.NED.dispatchEvent(DisconnectEvt);
            });
        }

        public void Close()
        {
            Channel?.Abort();
        }

    }
}
