using System.Net;
using System.Threading.Tasks;

namespace Bedrock.Framework
{
    public static class ClientFactory
    {
        public static async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint)
        {
            var conn = new SocketConnection(endpoint).StartAsync();
            return await conn.ConfigureAwait(false);
        }
    }
}
