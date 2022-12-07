using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace Geek.Server.Core.Net.Http
{
    public static class HttpServer
    {
        static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        static WebApplication app { get; set; }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="port"></param>
        public static Task Start(int httpPort, int httpsPort = 0)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                // HTTP 
                if (httpPort > 0)
                {
                    options.ListenAnyIP(httpPort);
                }

                // HTTPS
                if (httpsPort > 0)
                {
                    options.ListenAnyIP(httpsPort, builder =>
                    {
                        builder.UseHttps();
                    });
                }
            })
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Error);
            })
            .UseNLog();

            app = builder.Build();
            app.MapGet("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            app.MapPost("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            return app.StartAsync();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static Task Stop()
        {
            if (app != null)
            {
                Log.Info("停止http服务...");
                var task = app.StopAsync();
                app = null;
                return task;
            }
            return Task.CompletedTask;
        }
    }
}
