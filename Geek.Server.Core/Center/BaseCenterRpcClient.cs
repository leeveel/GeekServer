using Geek.Server.Core.Net;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Geek.Server.Core.Center
{
    public abstract class BaseCenterRpcClient : ICenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ICenterRpcHub ServerAgent { private set; get; }
        protected string connUrl;

        protected ReConnecter reConn;

        public BaseCenterRpcClient(string ip, int port)
        {
            connUrl = $"http://{ip}:{port}";
            reConn = new ReConnecter(ConnectImpl, $"中心服:{connUrl}");
        }

        public BaseCenterRpcClient(string url)
        {
            connUrl = url;
            reConn = new ReConnecter(ConnectImpl, $"中心服:{connUrl}");
        }

        public Task<bool> Connect()
        {
            return reConn.Connect();
        }

        private async Task<bool> ConnectImpl()
        {
            try
            {
                var channel = GrpcChannel.ForAddress(connUrl);
                ServerAgent = await StreamingHubClient.ConnectAsync<ICenterRpcHub, ICenterRpcClient>(channel, this);
                _ = RegisterDisconnectEvent();
                return true;
            }
            catch (Exception e)
            {
                LOGGER.Error($"rpc连接异常:{e.Message}");
                try
                {
                    if (ServerAgent != null)
                        await ServerAgent.DisposeAsync();
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"rpc.Dispose异常:{ex.Message}");
                }
                ServerAgent = null;
                return false;
            }
        }

        private async Task RegisterDisconnectEvent()
        {
            try
            {
                await ServerAgent.WaitForDisconnect();
            }
            catch (Exception e)
            {
                LOGGER.Error($"RegisterDisconnectEvent.rpc异常:{e.Message}");
            }
            finally
            {
                LOGGER.Info("disconnected from the center server.");
                await reConn.ReConnect();
            }
        }

        public abstract void ConfigChanged(ConfigInfo data);

        public abstract void NodesChanged(List<NetNode> nodes);

        public abstract void HaveMessage(string eid, string msg);
    }
}
