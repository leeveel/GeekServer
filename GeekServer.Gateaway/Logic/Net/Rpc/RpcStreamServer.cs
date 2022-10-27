using MagicOnion.Server.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeekServer.Gateaway.Net
{
    public class RpcStreamServer : StreamingHubBase<IRpcStreamServer, IRpcStreamClient>, IRpcStreamServer
    {

    }
}
