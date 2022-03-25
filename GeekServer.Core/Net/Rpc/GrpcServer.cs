using NLog;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;

namespace Geek.Server
{
    public class GrpcServer
    {

        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static Grpc.Core.Server server = null;

        public static void Init(int port)
        {
            string ip = GetLocalIp();

            LOGGER.Info($"grpc 服务器端启动. ip:{ip} port:{port}");

            server = new Grpc.Core.Server(new List<ChannelOption>
            {
                new ChannelOption("grpc.keepalive_time_ms", 800000), // 发送 keepalive 探测消息的频度
                new ChannelOption("grpc.keepalive_timeout_ms", 5000), // keepalive 探测应答超时时间
                new ChannelOption("grpc.keepalive_permit_without_calls", 1) // 是否允许在没有任何调用时发送 keepalive
            })
            {
                Services = { Inner.BindService(new GrpcHandlerImpl()) },
                Ports = { new ServerPort(ip, port, ServerCredentials.Insecure) }
            };
            server.Start();

            LOGGER.Info($"grpc 服务器端启动成功.");
        }

        private static string GetLocalIp()
        {
            string localIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = _IPAddress.ToString();
                    break;
                }
            }
            return localIP;
        }

        public static async Task Stop()
        {
            LOGGER.Info($"grpc 服务器端停止开始服务.");
            if (server != null) await server.ShutdownAsync();
            LOGGER.Info($"grpc 服务器端停止服务成功.");
        }
    }
}
