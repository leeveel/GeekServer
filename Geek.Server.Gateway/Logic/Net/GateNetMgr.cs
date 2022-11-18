using Geek.Server.Gateway.Logic.Net.Tcp.Outer;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Gateway.Logic.Net
{
    internal class GateNetMgr
    {
        public static Connections ClientConns { get; private set; } = new Connections();

       public static Connections ServerConns { get; private set; } = new Connections();

        public static CenterRpcClient CenterRpcClient { get; set; }

        public static async Task ConnectCenter()
        {
            CenterRpcClient = new CenterRpcClient();
            await CenterRpcClient.Connect(Settings.CenterUrl);
        }

        private static TcpServer outerTcpServer;
        private static TcpServer innerTcpServer;
        public static async Task StartTcpServer()
        {
            outerTcpServer = new TcpServer();
            await outerTcpServer.Start(Settings.TcpPort, builder => builder.UseConnectionHandler<OuterTcpConnectionHandler>());
            innerTcpServer = new TcpServer();
            await innerTcpServer.Start(Settings.InsAs<GateSettings>().InnerTcpPort, builder => builder.UseConnectionHandler<InnerTcpConnectionHandler>());
        }

        public static async Task StopTcpServer()
        {
            await outerTcpServer?.Stop();
            await innerTcpServer?.Stop();
        }

        public static long SelectAHealthNode(int serverId)
        {
            //TODO:分布式中一个大服的网络节点会存在多个
            //TODO:选择一个负载最小的节点
            var conn = ServerConns.GetByNodeId(serverId);
            if(conn != null)
                return conn.NodeId;
            return -1;
        }

        public static int GetConnectionCount()
        {
            return ClientConns.GetConnectionCount();
        }

    }
}
