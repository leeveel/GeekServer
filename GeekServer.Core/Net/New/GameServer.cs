using Bedrock.Framework;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Geek.Server
{
    public class GameServer
    {

        public static IWebHost Host { get; set; }

        public static WebApplication WebApp { get; set; }

        public static Task Start(IPEndPoint tcpEndPoint = null, IPEndPoint httpEndPoint=null, IPEndPoint httpsEndPoint=null)
        {
            if (tcpEndPoint == null && httpEndPoint == null && httpsEndPoint == null)
            {
                throw new ArgumentNullException("all endpoint is null");
            }
            Host = CreateWebHostBuilder(tcpEndPoint, httpEndPoint, httpsEndPoint).Build();
            return Host.StartAsync();
        }

        public static Task Start(int tcpPort=0, int httpPort=0, int httpsPort=0)
        {
            if (tcpPort <= 0 && httpPort <= 0 && httpsPort <= 0)
                throw new ArgumentNullException($"所有端口号都不合法{tcpPort},{httpPort},{httpsPort}");
            WebApp = CreateWebApplication(tcpPort, httpPort, httpsPort);
            return WebApp.StartAsync();
        }

        public static Task Stop()
        {
            if(WebApp != null)
                return WebApp.StopAsync();
            return Task.CompletedTask;
        }

        public static WebApplication CreateWebApplication(int tcpPort, int httpPort, int httpsPort)
        {
            var builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureServices(services =>
            {
                services.AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddConsole();
                });
            });
            builder.WebHost.UseKestrel(options =>
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

            });

            var app = builder.Build();
            app.MapGet("/", () => "Hello World!");
            //app.MapGet("/Test", Test);
            //app.Run();
            return app;
        }


        public static IWebHostBuilder CreateAnyIPWebHostBuilder(int tcpPort, int httpPort, int httpsPort)
        {
            var builder =  WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    });
                })
                .UseKestrel(options =>
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
                            // Use HTTP/3 (Quic)
                            //builder.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                            builder.UseHttps();
                        });
                    }

                })
                .UseStartup<Startup>();

            return builder;
        }

        public static IWebHostBuilder CreateWebHostBuilder(IPEndPoint tcpEndPoint, IPEndPoint httpEndPoint, IPEndPoint httpsEndPoint)
        {
            return WebHost.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    // This shows how a custom framework could plug in an experience without using Kestrel APIs directly
                    //services.AddFramework(new IPEndPoint(IPAddress.Loopback, 8009));
                    services.AddLogging(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug);
                        builder.AddConsole();
                    });
                })
                .UseKestrel(options =>
                {
                    // TCP 
                    if (tcpEndPoint != null)
                    {
                        options.Listen(tcpEndPoint, builder =>
                        {
                            //builder.UseConnectionHandler<MyEchoConnectionHandler>();
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
                //.UseQuic()
                .UseStartup<Startup>();
        }

    }
}
