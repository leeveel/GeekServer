using Geek.Server.Core.Center;
using Grpc.Net.Client;
using MagicOnion.Client;

namespace Geek.Server.App.Common
{
    public class CenterRpcClient : ICenterRpcClient
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public ICenterRpcHub ServerAgent { private set; get; }
        public CenterRpcClient() { }
        private string url;
        public async Task Connect(string url)
        {
            try
            {
                this.url = url; 
                var channel = GrpcChannel.ForAddress(url);
                ServerAgent = await StreamingHubClient.ConnectAsync<ICenterRpcHub, ICenterRpcClient>(channel, this);
                RegisterDisconnectEvent();
            }
            catch (Exception e)
            {
                try
                {
                    if (ServerAgent != null)
                        await ServerAgent.DisposeAsync();
                }
                catch (Exception)
                {

                }
                ServerAgent = null;
                LOGGER.Error($"rpc异常:{e.Message}");
                //临时处理，后续多个网关，通过服务发现制定重连策略
                await Task.Delay(2000);
                _ = Connect(url);
            }
        }

        private async void RegisterDisconnectEvent()
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
                //TODO 清理所有session，连上后通知网关清理老客户端
                LOGGER.Info("disconnected from the server.");
                await Task.Delay(2000);
                await Connect(url);
            }
        }

        public void Connected(long uid)
        {
            LOGGER.Debug($"新的客户端连接:{uid}");
        }

        public void Disconnected(long uid)
        {
            LOGGER.Debug($"移除客户端连接:{uid}");
        }

        public void ConfigChanged(byte[] data)
        {
            Console.WriteLine("ConfigChanged:" + data);
        }

        public void NodesChanged(List<NetNode> nodes)
        {
            foreach (var node in nodes)
            {
                Console.WriteLine("NodesChanged:" + node.NodeId);
                if (node.Type == NodeType.Gateway)
                {
                    _ = NetHelper.ConnectGateway(node);
                }
            }
            Console.WriteLine("---------------------------------");
        }

    }
}
