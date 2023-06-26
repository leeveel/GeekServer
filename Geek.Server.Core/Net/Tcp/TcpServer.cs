using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Geek.Server.Core.Net.Tcp
{
    /// <summary>
    /// TCP server
    /// </summary>
    public static class TcpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        static WebApplication app { get; set; }

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
                    builder.UseConnectionHandler<TcpConnectionHandler>();
                });
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Error);
            })
            .UseNLog();

            var app = builder.Build();
            return app.StartAsync();
        }

        public static Task Start(int port, Action<ListenOptions> configure)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(port, configure);
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Error);
            })
            .UseNLog();

            app = builder.Build();
            Log.Info("启动tcp服务...");
            return app.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (app != null)
            {
                Log.Info("停止Tcp服务...");
                var task = app.StopAsync();
                app = null;
                return task;
            }
            return Task.CompletedTask;
        }
    }
}
