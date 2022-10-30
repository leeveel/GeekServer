
using GeekServer.Gateaway.Net.Router;
using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net.Rpc
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
        public long defaultTargetUid => 0;

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

        public Task<long> SetInfo(long serverId, int type)
        {
            this.serverId = serverId;
            this.serverType = (NodeType)type;
            //生成uid

            return Task.FromResult(uid);
        }

        public void Write(long fromId, int msgId, byte[] data)
        {
            BroadcastToSelf(group).Revice(fromId, msgId, data);
        }

        //请求路由消息
        public void Router(long targetUid, int msgId, byte[] data)
        {
            MsgRouter.To(this, targetUid, msgId, data);
        }
    }
}
