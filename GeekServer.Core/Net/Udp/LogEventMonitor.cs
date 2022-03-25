using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Geek.Server.Udp
{

    /// <summary>
    /// udp服务器
    /// </summary>
    public class LogEventMonitor
    {
        private readonly IEventLoopGroup _group;
        private readonly Bootstrap _bootstrap;
        private readonly int _port;

        public LogEventMonitor(int port)
        {
            _group = new MultithreadEventLoopGroup();
            _bootstrap = new Bootstrap();
            _bootstrap
                .Group(_group)
                // 引导该NioDatagramChannel
                .Channel<SocketDatagramChannel>()
                // 设置套接字选项SO_BROADCAST
                .Option(ChannelOption.SoBroadcast, true)
                .Handler(new ActionChannelInitializer<DotNetty.Transport.Channels.IChannel>(channel =>
                {
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new LogEventDecoder());
                    pipeline.AddLast(new LogEventHandler());
                }));
            _port = port;
        }

        public Task<DotNetty.Transport.Channels.IChannel> Bind()
        {
            return _bootstrap.BindAsync(_port);
        }

        public async Task Stop()
        {
            await _group.ShutdownGracefullyAsync();
        }

        public static async Task Start(int port)
        {
            LogEventMonitor monitor = new LogEventMonitor(port);
            try
            {
                DotNetty.Transport.Channels.IChannel channel = await monitor.Bind();
                Console.WriteLine("LogEventMonitor running");
                Console.ReadKey();
                await channel.CloseAsync();
            }
            finally
            {
                await monitor.Stop();
            }
        }
    }
}
