using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Common;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;

namespace Geek.Server
{
    public class HttpServer
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        static IEventLoopGroup bossGroup;
        static IEventLoopGroup workerGroup;
        static DotNetty.Transport.Channels.IChannel bootstrapChannel;

        static HttpServer()
        {
            ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
        }

        static async Task RunHttpServerAsync(int port)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

            bossGroup = new MultithreadEventLoopGroup(1);
            workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(bossGroup, workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap.Option(ChannelOption.SoBacklog, 8192);
                bootstrap.ChildHandler(new ActionChannelInitializer<DotNetty.Transport.Channels.IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536 * 5));
                        pipeline.AddLast(new HttpDecoder());
                    }));

                bootstrapChannel = await bootstrap.BindAsync(port);
                LOGGER.Info("start http server success. listener port:[{}]", port);
            }
            catch (Exception e)
            {
                throw new Exception("start http server ERROR! \n" + e.ToString());
            }
        }

        public static Task Start(int port)
        {
            return RunHttpServerAsync(port);
        }

        public static async Task Stop()
        {
            await Task.WhenAll(
                bootstrapChannel.CloseAsync(),
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2)));
            LOGGER.Info("http server stoped");
        }
    }
}
