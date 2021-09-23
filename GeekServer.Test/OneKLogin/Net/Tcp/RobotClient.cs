using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Geek.Server.Test
{
    public class RobotClient
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        public static int Port { private set; get; }
        public static string Host { private set; get; }

        private static Bootstrap bootstrap;
        public static void Init(string host, int port, List<Type> handlerList)
        {
            Host = host;
            Port = port;
            try
            {
                bootstrap = new Bootstrap();
                bootstrap.Group(new MultithreadEventLoopGroup());
                bootstrap.Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.SoReuseaddr, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5))
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    if (handlerList != null && handlerList.Count > 0)
                    {
                        for (int i = 0; i < handlerList.Count; i++)
                            pipeline.AddLast(Activator.CreateInstance(handlerList[i]) as IChannelHandler);
                    }
                }));
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
                throw e;
            }
        }

        public static async Task<DotNetty.Transport.Channels.IChannel> Connect()
        {
            try
            {
                //var serverHost = new IPEndPoint(IPAddress.Parse(Host), Port);
                //long startTime = TimeUtils.CurrentTimeMillis();
                var channel = await bootstrap.ConnectAsync(IPAddress.Parse(Host), Port);
                LOGGER.Info("tcp connect success>{}:{}", Host, Port);
                //Console.WriteLine("connect耗时：" + (TimeUtils.CurrentTimeMillis() - startTime));
                return channel;
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
                throw e;
            }
        }

        public static Task Close()
        {
            if (bootstrap != null)
                return bootstrap.Group().ShutdownGracefullyAsync();
            return Task.CompletedTask;
        }

    }
}
