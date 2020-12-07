/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DotNetty.Codecs.Http;
using DotNetty.Common;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using NLog;

namespace Geek.Core.Net.Http
{
    /// <summary>
    /// HTTP server
    /// </summary>
    public class HttpServer
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        private static IEventLoopGroup m_bossGroup;
        private static IEventLoopGroup m_workerGroup;
        private static IChannel bootstrapChannel;

        static HttpServer()
        {
            ResourceLeakDetector.Level = ResourceLeakDetector.DetectionLevel.Disabled;
        }

        static async Task RunHttpServerAsync(int port, Func<HttpDecoder> func)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            m_bossGroup = new MultithreadEventLoopGroup(1);
            m_workerGroup = new MultithreadEventLoopGroup();

            try
            {
                var bootstrap = new ServerBootstrap();
                bootstrap.Group(m_bossGroup, m_workerGroup);
                bootstrap.Channel<TcpServerSocketChannel>();
                bootstrap
                    .Option(ChannelOption.SoBacklog, 8192)
                    .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                    {
                        IChannelPipeline pipeline = channel.Pipeline;

                        pipeline.AddLast(new HttpServerCodec());
                        pipeline.AddLast(new HttpObjectAggregator(65536 * 5));
                        pipeline.AddLast(func());
                    }));

                bootstrapChannel = await bootstrap.BindAsync(port);
                LOGGER.Info("start http server success. listener port:[{}]", port);
            }
            catch (Exception e)
            {
                LOGGER.Error(e, e.Message);
                throw new Exception("start http server ERROR! \n" + e.StackTrace);
            }
        }

        public static void Start(int port)
        {
            Func<HttpDecoder> func = () => new HttpServerHandler();
            RunHttpServerAsync(port, func).Wait();
        }

        public static void Stop()
        {
            bootstrapChannel.CloseAsync();
            m_bossGroup.ShutdownGracefullyAsync();
            m_workerGroup.ShutdownGracefullyAsync();
            LOGGER.Info("stop http server success.");
        }
    }
}
