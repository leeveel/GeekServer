using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using System.Diagnostics;

namespace Geek.Server.Core.Net.Tcp
{
    public partial class NetChannel
    {
        public long Id { get; set; } = 0;
        public long NodeId { get; set; } = 0;
    }
}
