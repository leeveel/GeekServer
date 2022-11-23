using System.Net;
using Geek.Server.Core.Net.Bedrock.Transports.Sockets;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Bedrock.Client
{
    public static class ClientFactory
    {
        public static async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint, int timeoutMS=5000)
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(timeoutMS));
                var conn = new SocketConnection(endpoint, cts.Token).StartAsync();
                return await conn.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                if (e is System.OperationCanceledException)
                    throw new ConnectTimeoutException($"connection timed out in {timeoutMS}ms:{endpoint}");
                else
                    throw;
            }
        }
    }
}
