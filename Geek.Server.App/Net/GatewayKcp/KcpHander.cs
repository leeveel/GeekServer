using Geek.Server.App.Common.Handler;
using Geek.Server.App.Net.Session;
using Geek.Server.Core.Hotfix;
using Geek.Server.Core.Net.Tcp.Handler;
using Geek.Server.Core.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using NLog;
using Geek.Server.Core.Net.Kcp;

namespace Geek.Server.App.Net.GatewayKcp
{
    public class KcpHander
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        public static async Task OnMessage(BaseNetChannel chann, Message msg)
        {
            //LOGGER.Debug($"处理消息:{msg.GetType().Name}");
            var handler = HotfixMgr.GetTcpHandler(msg.MsgId);
            if (handler == null)
            {
                LOGGER.Error($"找不到[{msg.MsgId}]对应的handler");
                return;
            }

            handler.Channel = chann;
            handler.Msg = msg;
            handler.ClientNetId = chann.NetId;

            if (handler is BaseCompHandler compHander)
            {
                await compHander.InitActor();
            }

            _ = handler.InnerAction();
        }

        public static void OnChannelRemove(KcpChannel channel)
        {
            if (channel == null)
                return;
            var session = channel.GetData<GameSession>(SessionManager.SESSION);
            if (session != null)
            {
                SessionManager.Remove(session);
            }
        }
    }
}
