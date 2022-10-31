
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Geek.Server
{
    public class RpcServer
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
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
                    logging.SetMinimumLevel(LogLevel.Error);
                }).UseNLog();
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
