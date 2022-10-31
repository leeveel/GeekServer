using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class StreamClient : IStreamClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        string url;
        public IStreamServer serverAgent;

        internal static readonly ConcurrentDictionary<long, Session> sessionMap = new();

        GrpcChannel channel;
        int selfServerId;
        int selfServerType;
        public StreamClient(string url, int selfServerId, int selfServerType)
        {
            this.url = url;
            this.selfServerId = selfServerId;
            this.selfServerType = selfServerType;
            channel = GrpcChannel.ForAddress(url);
        }

        public async Task Connect()
        {
            serverAgent = await StreamingHubClient.ConnectAsync<IStreamServer, IStreamClient>(channel, this);
            await serverAgent.SetInfo(selfServerId, selfServerType);
            RegisterDisconnectEvent();
        }

        private async void RegisterDisconnectEvent()
        {
            try
            {
                await serverAgent.WaitForDisconnect();
            }
            catch (Exception e)
            {

            }
            finally
            {
                LOGGER.Info("disconnected from the server.");
                await Task.Delay(2000);
                await Connect();
            }
        }

        public async void Revice(long fromUid, int msgId, byte[] data)
        {
            sessionMap.TryGetValue(fromUid, out var sess);
            if (sess == null)
                return;
            var msg = MessagePack.MessagePackSerializer.Deserialize<Message>(data);
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}][{msg.GetType()}]对应的handler");
                return;
            }
            handler.Msg = msg;
            handler.Session = sess;
            await handler.Init();
            await handler.InnerAction();
        }

        public void PlayerConnect(long uid)
        {
            sessionMap.TryGetValue(uid, out var sess);
            if (sess != null)
            {
                sess.netId = uid;
                sess.streanClient = this;
                return;
            }
            sess = new Session();
            sess.netId = uid;
            sess.streanClient = this;
            sessionMap.TryAdd(uid, sess);

            LOGGER.Debug($"新的客户端连接:{uid}");
        }
        public void PlayerDisconnect(long uid)
        {
            LOGGER.Debug($"移除客户端连接:{uid}");
            sessionMap.TryRemove(uid, out _);
            HotfixMgr.SessionMgr.Remove(uid);
            serverAgent.DisconnectNode(uid);
        }
    }
}
