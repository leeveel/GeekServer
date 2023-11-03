using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Geek.Server.Core.Net.Rpc
{
    public class RpcServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private class LoggerProvider : ILoggerProvider
        {
            readonly NLogLoggerFactory loggerFactory;
            public LoggerProvider()
            {
                loggerFactory = new NLogLoggerFactory();
            }

            public void Dispose()
            {
            }

            public ILogger CreateLogger(string categoryName)
            {
                return loggerFactory.CreateLogger(categoryName);
            }
        }

        public static IHost host { get; set; }
        public static Task Start(int rpcPort)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                    .UseKestrel(options =>
                    {
                        options.ListenAnyIP(rpcPort, listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                        });
                    })
                      .UseStartup<RpcStartup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Warning).AddProvider(new LoggerProvider());
                });
            host = builder.Build();
            return host.StartAsync();
        }

        public static Task Stop()
        {
            Log.Info("停止rpc服务...");
            return host.StopAsync();
        }
    }
}
