using System;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;

namespace Geek.Server
{
    /// <summary>
    /// TCP server
    /// </summary>
    public class TcpServer
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        static IEventLoopGroup bossGroup;
        static IEventLoopGroup workerGroup;
        static DotNetty.Transport.Channels.IChannel bootstrapChannel;

        static async Task RunServerAsync(int port, bool useLibuv)
        {
            if (useLibuv)
            {
                DispatcherEventLoopGroup dispatcher = new DispatcherEventLoopGroup();
                bossGroup = dispatcher;
                workerGroup = new WorkerEventLoopGroup(dispatcher);
            }
            else
            { 
                bossGroup = new MultithreadEventLoopGroup(1);
                workerGroup = new MultithreadEventLoopGroup();  //默认为CPU核数*2
            }
            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);

                if (useLibuv)
                    bootstrap.Channel<TcpServerChannel>();
                else
                    bootstrap.Channel<TcpServerSocketChannel>();
                
                bootstrap.Option(ChannelOption.SoBacklog, 65535)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildOption(ChannelOption.SoKeepalive, true)
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<DotNetty.Transport.Channels.IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast("IdleChecker", new IdleStateHandler(50, 50, 0));
                    // 消息编码解码器 分发handler
                    //每个channel channelpipeline 都会new一次，即每个客户端
                    pipeline.AddLast(new TcpServerEncoder(), new TcpServerDecoder(), new TcpServerHandler());
                }));

                bootstrapChannel = await bootstrap.BindAsync(port);
                LOGGER.Info("start tcp server success. listener port:[{}]", port);
            }
            catch (Exception e)
            {
                LOGGER.Error(e, e.Message);
                throw new Exception("start tcp server ERROR! \n" + e.StackTrace);
            }
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <param name="port"></param>
        public static Task Start(int port, bool useLibuv)
        {
            return RunServerAsync(port, useLibuv);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public static async Task Stop()
        {
            await Task.WhenAll(
                bootstrapChannel.CloseAsync(),
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            LOGGER.Info("tcp server stoped");
        }
    }
}
