using Core.Discovery;
using Geek.Server.Core.Discovery;
using Geek.Server.Gateway.Common;

namespace Geek.Server.Gateway
{
    internal class GatewayDiscoveryClient : BaseDiscoveryClient
    {
        static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        static GatewayDiscoveryClient _instance;
        public static GatewayDiscoveryClient Instance
        {
            get
            {
                _instance ??= new GatewayDiscoveryClient();
                return _instance;
            }
        }

        public GatewayDiscoveryClient() :
            base(
                () => new ServerInfo
                {
                    ServerId = Settings.Ins.ServerId, 
                    LocalIp = Settings.Ins.LocalIp,
                    ServerName = Settings.Ins.ServerName, 
                    InnerPort = Settings.Ins.InnerPort,
                    OuterPort = Settings.Ins.OuterPort,
                    HttpPort = Settings.Ins.HttpPort,
                    Type = ServerType.Gate,
                },
                () => new ServerState
                {
                    MaxLoad = Settings.InsAs<GateSettings>().MaxClientCount,
                    CurrentLoad = GateServer.Instance.CurActiveChannelCount
                })
        {
            UpdateAllGateway();
        }

        public async void UpdateAllGateway()
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var token = closeTokenSrc.Token;
            //定时同步其他网关信息
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (ServerAgent != null)
                    {
                        var list = await ServerAgent.GetNodesByType(ServerType.Gate);
                        foreach (var info in list)
                        {
                            var oldInfo = GetServer(info.ServerId);
                            if (oldInfo != null)
                            {
                                oldInfo.State = info.State;
#if DEBUG
                                LOGGER.Debug($"更新server[{info.ServerId}][{info.Type}]状态,{info.State?.ToString()}");
#endif
                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromSeconds(15), token);
                }
                catch
                {

                }
            }
        }

        public ServerInfo GetIdleGateway()
        {
            foreach (var kv in gatewayServerMap)
            {
                var sInfo = kv.Value;
                if (sInfo.State.CurrentLoad < sInfo.State.MaxLoad)
                {
                    return sInfo;
                }
            }
            return null;
        }
    }
}
