using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.BackendServer
{
    //一个客户端连接对应一个server实例
    public class StreamServer : StreamingHubBase<IStreamServer, IStreamClient>, IStreamServer
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        const string globalGroupName = "global";

        private IGroup group;

        private long serverId = 0;
        private int serverType = 0;

        protected override async ValueTask OnConnecting()
        {
            LOGGER.Debug($"rpc客户端连接:{Context.CallContext.Peer}");
            group = await Group.AddAsync(globalGroupName);
        }

        protected override ValueTask OnDisconnected()
        {
            LOGGER.Debug($"rpc客户端断开连接:{Context.CallContext.Peer}");
            if (serverId > 0)
                ServerNodeMgr.Remove(serverId);
            if (group != null)
                group.RemoveAsync(Context);
            group = null;
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// 后端服务器连接上后,需要主动告知服务器信息
        /// </summary>
        public void GameServerSetInfo(long serverId, int type)
        {
            this.serverId = serverId;
            this.serverType = type;
            ServerNodeMgr.Add(serverId, this);
        }
    }
}
