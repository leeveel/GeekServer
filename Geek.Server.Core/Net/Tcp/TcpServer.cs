using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Connections;

namespace Geek.Server.Core.Net.Tcp
{
    /// <summary>
    /// TCP server
    /// </summary>
    public class TcpServer<T> where T : ConnectionHandler
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public WebApplication app { get; set; }

        public Task Start(int port)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(port, builder =>
                {
                    builder.UseConnectionHandler<T>();
                });
            })
            .UseNLog();

            app = builder.Build();
            return app.StartAsync();
        }

        public Task Stop()
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
