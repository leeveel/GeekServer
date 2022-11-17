using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Connections;
using NLog.Web;
using Geek.Server.Gateway.Logic.Net.Tcp.Outer;

namespace Geek.Server.Gateway.Logic.Net.Codecs
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class OuterTcpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static WebApplication WebApp { get; set; }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="port"></param>
        public static Task Start(int port)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(port, builder =>
                {
                    builder.UseConnectionHandler<OuterTcpConnectionHandler>();
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Error);
            })
            .UseNLog();

            var app = builder.Build();
            return app.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (WebApp != null)
            {
                Log.Info("停止Tcp服务...");
                var task = WebApp.StopAsync();
                WebApp = null;
                return task;
            }
            return Task.CompletedTask;
        }
    }
}
