using Grpc.Net.Client;
using MagicOnion.Client;

namespace Geek.Server.Core.Center
{
    public abstract class BaseCenterRpcClient : ICenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        public ICenterRpcHub ServerAgent { private set; get; }
        private string url;
        public async Task Connect(string url)
        {
            try
            {
                this.url = url;
                var channel = GrpcChannel.ForAddress(url);
                ServerAgent = await StreamingHubClient.ConnectAsync<ICenterRpcHub, ICenterRpcClient>(channel, this);
                _ = RegisterDisconnectEvent();
            }
            catch (Exception e)
            {
                LOGGER.Error($"rpc连接异常:{e}");
                try
                {
                    await ServerAgent?.DisposeAsync();
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"rpc.Dispose异常:{ex}");
                }
                ServerAgent = null;
                await ReConnect();
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
                await ReConnect();
            }
        }

        private async Task ReConnect()
        {
            int delay = 15000;
            LOGGER.Error($"连接断开,{delay}ms后尝试重连");
            await Task.Delay(delay);
            await Connect(url);
        }

        public abstract void ConfigChanged(byte[] data);

        public abstract void NodesChanged(List<NetNode> nodes);
    }
}
