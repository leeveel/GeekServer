namespace Geek.Server.Gateway.Logic.Net
{
    internal class GateNetHelper
    {
        public static Connections ClientConns { get; private set; } = new Connections();

       public static Connections ServerConns { get; private set; } = new Connections();

        public static CenterRpcClient CenterRpcClient { get; set; }

        public static async Task ConnectCenter()
        {
            CenterRpcClient = new CenterRpcClient();
            await CenterRpcClient.Connect(Settings.CenterUrl);
        }

        public static long SelectAHealthNode(int serverId)
        {
            //TODO:选择一个负载最小的节点
            var nodes = ServerConns.GetAllNodes();
            foreach (var node in nodes)
            {
                if (node.Id == serverId)
                {
                    return node.Id;
                }
            }
            return -1;
        }

        public static int GetNodeCount()
        {
            return ClientConns.GetNodeCount();
        }

    }
}
