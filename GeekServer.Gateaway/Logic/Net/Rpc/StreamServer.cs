
using Bedrock.Framework.Protocols;
using Consul;
using GeekServer.Gateaway.Net.Router;
using GeekServer.Gateaway.Net.Tcp;
using MagicOnion.Server.Hubs;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Geek.Server
{
    //一个客户端连接对应一个server实例
    public class StreamServer : StreamingHubBase<IStreamServer, IStreamClient>, IStreamServer, INetNode
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const string globalGroupName = "global";

        private IGroup group;

        private long serverId = 0;
        private NodeType serverType = 0;

        public long uid => serverId;
        public NodeType type => serverType;
        public long defaultTargetUid { get => 0; set { } }

        protected override async ValueTask OnConnecting()
        {
            LOGGER.Debug($"rpc客户端连接:{Context.CallContext.Peer}");
            group = await Group.AddAsync(globalGroupName);
        }


        protected override ValueTask OnDisconnected()
        {
            LOGGER.Debug($"rpc客户端断开连接:{Context.CallContext.Peer}");
            NetNodeMgr.Remove(uid);
            if (group != null)
                group.RemoveAsync(Context);
            group = null;
            return ValueTask.CompletedTask;
        }
        public IStreamClient GetStreamClientAgent()
        {
            return BroadcastToSelf(group);
        }

        public void Write(long fromId, int msgId, byte[] data)
        {
            BroadcastToSelf(group).Revice(fromId, msgId, data);
        }

        public Task<long> SetInfo(int serverId, int type)
        {
            this.serverId = serverId;
            this.serverType = (NodeType)type;
            NetNodeMgr.Add(this);
            return Task.FromResult(uid);
        }

        //服务器主动要求断开客户端连接
        public Task DisconnectNode(long targetUid)
        {
            var node = NetNodeMgr.Remove(targetUid);
            if (node != null)
            {
                node.Abort();
            }
            return Task.CompletedTask;
        }

        //请求路由消息
        public Task Router(long targetUid, int msgId, byte[] data)
        {
            MsgRouter.To(this, targetUid, msgId, data);
            return Task.CompletedTask;
        }
    }
}
