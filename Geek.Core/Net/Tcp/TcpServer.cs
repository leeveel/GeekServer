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
using NLog;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using System.Collections.Generic;

namespace Net.Tcp
{
    /// <summary>
    /// TCP server
    /// </summary>
    public class TcpServer
    {
        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        private IEventLoopGroup bossGroup;
        private IEventLoopGroup workerGroup;
        private IChannel bootstrapChannel;

        public int Port { private set; get; }

        public bool UseLibuv { private set; get; }

        public TcpServer(int port, bool useLibuv)
        {
            Port = port;
            UseLibuv = useLibuv;
        }

        public async Task RunServerAsync(List<Type> handlerList)
        {
            //bool useLibuv = bool.Parse(Setting.Get<string>(EServer.Gate, SE.UseLibuv));
            if (UseLibuv)
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

                if (UseLibuv)
                    bootstrap.Channel<TcpServerChannel>();
                else
                    bootstrap.Channel<TcpServerSocketChannel>();
                
                bootstrap.Option(ChannelOption.SoBacklog, 65535)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildOption(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .ChildOption(ChannelOption.SoKeepalive, true)
                .ChildOption(ChannelOption.TcpNodelay, true)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast("IdleChecker", new IdleStateHandler(50, 50, 0));
                    // 消息编码解码器 分发handler
                    //每个channel channelpipeline 都会new一次，即每个客户端
                    //pipeline.AddLast(new ServerEncoder(), new ServerDecoder(), new ServerHandler());
                    if (handlerList != null && handlerList.Count > 0)
                    {
                        for (int i = 0; i < handlerList.Count; i++)
                            pipeline.AddLast(Activator.CreateInstance(handlerList[i]) as IChannelHandler);
                    }
                }));

                bootstrapChannel = await bootstrap.BindAsync(Port);
                LOGGER.Info("start tcp server success. listener port:[{}]", Port);
                //Console.ReadLine();
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
        public void Start(List<Type> handlerList)
        {
            RunServerAsync(handlerList).Wait();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public async void Stop()
        {
            LOGGER.Info("stopping tcp server...");
            await bootstrapChannel.CloseAsync();
            await Task.WhenAll(
                        bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                        workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }

    }

}
