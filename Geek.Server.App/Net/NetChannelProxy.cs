using Common.Net.Tcp;
using Geek.Server.App.Net.Session;
using Geek.Server.Proto;
using NLog;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Geek.Server.App.Net
{
    public class NetChannelProxy : INetChannel
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        InnerTcpClient innerTcpClient;
        public NetChannelProxy(InnerTcpClient innerTcpClient, long netId, string address)
        {
            NetId = netId;
            this.innerTcpClient = innerTcpClient;
            RemoteAddress = address;
        }
        public ConcurrentDictionary<string, object> Datas { get; set; } = new();
        public long NetId { get; set; }
        public string RemoteAddress { get; set; }
        public long ResCode { get; set; }
        public long DefaultTargetNodeId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        bool closed = false;

        public async ValueTask Write(object msg)
        {
            if (closed)
                return;
            var realMsg = msg as Message;
            if (realMsg == null)
            {
                LOGGER.Error($"写入Message消息类型错误,{msg.GetType().FullName}");
                return;
            }
            realMsg.NetId = NetId;
            var channel = innerTcpClient.Channel;
            if (channel != null)
                await channel.Write(realMsg);
        }

        public T GetData<T>(string key)
        {
            if (Datas.TryGetValue(key, out var v))
            {
                return (T)v;
            }
            return default;
        }

        public void SetData(string key, object v)
        {
            Datas[key] = v;
        }

        public async void Close()
        {
            LOGGER.Debug($"关闭net proxy,NetId:{NetId}");
            innerTcpClient.RemoveProxy(NetId);
            await Write(new ReqDisconnectClient { NetId = NetId });
            closed = true;
            var session = GetData<GameSession>(SessionManager.SESSION);
            SetData(SessionManager.SESSION, null);
            if (session != null)
                SessionManager.Remove(session);
        }

        public bool IsClose()
        {
            return closed || innerTcpClient.GetProxy(NetId) == null;
        }
    }
}
