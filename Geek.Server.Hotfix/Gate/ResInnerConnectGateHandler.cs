using Geek.Server.Core.Net.Tcp.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Logic.Gate
{
    [MsgMapping(typeof(ResInnerConnectGate))]
    public class ResInnerConnectGateHandler : BaseTcpHandler
    {
        public override Task ActionAsync()
        {
            Console.WriteLine("ResInnerConnectGateHandler success");
            return Task.CompletedTask;
        }
    }
}
