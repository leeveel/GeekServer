using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class GameServer
    {
        public static WebApplication WebApp { get; set; }

        public static Task Start(IPEndPoint tcpEndPoint = null, IPEndPoint httpEndPoint = null, IPEndPoint httpsEndPoint = null)
        {
            if (tcpEndPoint == null && httpEndPoint == null && httpsEndPoint == null)
            {
                throw new ArgumentNullException("all endpoint is null");
            }
            WebApp = CreateWebHostBuilder(tcpEndPoint, httpEndPoint, httpsEndPoint);
            return WebApp.StartAsync();
        }

        public static Task Start(int tcpPort = 0, int httpPort = 0, int httpsPort = 0)
        {
            if (tcpPort <= 0 && httpPort <= 0 && httpsPort <= 0)
                throw new ArgumentNullException($"所有端口号都不合法{tcpPort},{httpPort},{httpsPort}");
            WebApp = CreateWebApplication(tcpPort, httpPort, httpsPort);
            return WebApp.StartAsync();
        }

        public static Task Stop()
        {
            if (WebApp != null)
                return WebApp.StopAsync();
            return Task.CompletedTask;
        }


        public static WebApplication CreateWebApplication(int tcpPort, int httpPort, int httpsPort)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
            {
                // TCP 
                if (tcpPort > 0)
                {
                    options.ListenAnyIP(tcpPort, builder =>
                    {
                        //builder.UseConnectionLogging();
                        builder.UseConnectionHandler<TcpConnectionHandler>();
                    });
                }

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
                if(Settings.Ins.IsDebug)
                    logging.SetMinimumLevel(LogLevel.Information);
                else
                    logging.SetMinimumLevel(LogLevel.None);
            })
            .UseNLog();

            var app = builder.Build();
            app.MapGet("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            app.MapPost("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            return app;
        }

        public static WebApplication CreateWebHostBuilder(IPEndPoint tcpEndPoint, IPEndPoint httpEndPoint, IPEndPoint httpsEndPoint)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.UseKestrel(options =>
            {
                // TCP 
                if (tcpEndPoint != null)
                {
                    options.Listen(tcpEndPoint, builder =>
                    {
                        //builder.UseConnectionLogging();
                        builder.UseConnectionHandler<TcpConnectionHandler>();
                    });
                }

                // HTTP 
                if (httpsEndPoint != null)
                {
                    options.Listen(httpEndPoint);
                }

                // HTTPS
                if (httpsEndPoint != null)
                {
                    options.Listen(httpsEndPoint, builder =>
                    {
                        // Use HTTP/3 (Quic)
                        //builder.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                        builder.UseHttps();
                    });
                }

            })
            .ConfigureLogging(logging =>
            {
                if (Settings.Ins.IsDebug)
                    logging.SetMinimumLevel(LogLevel.Information);
                else
                    logging.SetMinimumLevel(LogLevel.None);
            })
            .UseNLog();

            var app = builder.Build();
            app.MapGet("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            app.MapPost("/game/{text}", (HttpContext context) => HttpHandler.HandleRequest(context));
            return app;
        }

    }
}
