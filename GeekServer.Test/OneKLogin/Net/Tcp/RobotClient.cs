using Bedrock.Framework;
using Bedrock.Framework.Protocols;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class RobotClient
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public static int Port { private set; get; }
        public static string Host { private set; get; }

        public static void Init(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public static async Task<ClientNetChannel> Connect()
        {
            try
            {
                var loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                    builder.AddConsole();
                });

                var client = new ClientBuilder()
                                        .UseSockets()
                                        //.UseConnectionLogging(loggerFactory: loggerFactory)
                                        .Build();
                var connection = await client.ConnectAsync(new IPEndPoint(IPAddress.Parse(Host), Port));
                LOGGER.Debug($"Connected to {connection.LocalEndPoint}");
                return new ClientNetChannel(connection, new ClientLengthPrefixedProtocol());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void Close(NetChannel channel)
        {
            channel?.Abort();
        }

    }
}
