using Geek.Server.Core.Center;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Rebalance
{
    internal static class GatewayMgr
    {
        static ConcurrentDictionary<long, NetNode> gatewayMap = new();

        public static void ResetAllNode(List<NetNode> gateNodes)
        {
            gatewayMap.Clear();
            foreach (var n in gateNodes)
            {
                gatewayMap[n.NodeId] = n;
            }
        }

        public static void UpdateNode(NetNode node)
        {
            gatewayMap[node.NodeId] = node;
        }

        public static NetNode GetIdleGate()
        {
            NetNode node = null;
            var curLoadCount = int.MaxValue;
            foreach (var n in gatewayMap)
            {
                var state = n.Value.State;
                if (state != null && state.CanServe && state.MaxLoad > state.CurrentLoad && state.CurrentLoad <= curLoadCount)
                {
                    curLoadCount = state.CurrentLoad;
                    node = n.Value;
                }
            }
            return node;
        }
    }
}
