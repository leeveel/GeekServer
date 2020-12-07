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
using NLog;
using System;
using DotNetty.Buffers;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetty.Transport.Channels.Sockets;
using System.Net;

namespace Net.Tcp
{
    public class TcpClient
    {
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        int port;
        string host;
        public TcpClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
        Bootstrap bootstrap;
        public async Task<IChannel> Connect(List<Type> handlerList)
        {
            try
            {
                bootstrap = new Bootstrap();
                bootstrap.Group(new MultithreadEventLoopGroup(1));
                bootstrap.Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Option(ChannelOption.SoKeepalive, true)
                .Option(ChannelOption.ConnectTimeout, TimeSpan.FromSeconds(5))
                .Option(ChannelOption.Allocator, PooledByteBufferAllocator.Default)
                .Option(ChannelOption.RcvbufAllocator, new AdaptiveRecvByteBufAllocator())
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    //消息编码解码器 分发handler
                    //每个channel channelpipeline 都会new一次，即每个客户端
                    if (handlerList != null && handlerList.Count > 0)
                    {
                        for (int i = 0; i < handlerList.Count; i++)
                            pipeline.AddLast(Activator.CreateInstance(handlerList[i]) as IChannelHandler);
                    }
                }));
                var serverHost = new IPEndPoint(IPAddress.Parse(host), port);
                var channel = await bootstrap.ConnectAsync(serverHost);
                LOGGER.Info(string.Format("tcp connect success>{0}:{1}", host, port));
                return channel;
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
                //throw e;
                return default;
            }
        }

        public async Task<IChannel> ReConnect()
        {
            try
            {
                var serverHost = new IPEndPoint(IPAddress.Parse(host), port);
                var channel = await bootstrap.ConnectAsync(serverHost);
                return channel;
            }
            catch (Exception e)
            {
                LOGGER.Error(e.ToString());
                return default;
            }
        }

        public Task Close()
        {
            if (bootstrap != null)
                return bootstrap.Group().ShutdownGracefullyAsync();
            return Task.CompletedTask;
        }
    }
}
