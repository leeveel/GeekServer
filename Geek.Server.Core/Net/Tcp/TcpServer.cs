using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Geek.Server.Core.Net.Tcp
{
    /// <summary>
    /// TCP server
    /// </summary>
    public class TcpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public WebApplication app { get; set; }

        public Task Start(int port, Action<ListenOptions> configure)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                options.ListenAnyIP(port, configure);
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
