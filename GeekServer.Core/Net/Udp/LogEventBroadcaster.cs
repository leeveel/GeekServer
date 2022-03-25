using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Geek.Server.Udp
{

    /// <summary>
    /// udp客户端
    /// </summary>
    public class LogEventBroadcaster
    {

        private readonly IEventLoopGroup _group;
        private readonly Bootstrap _bootstrap;
        private readonly string _fileName;

        public LogEventBroadcaster(EndPoint address, string fileName)
        {
            _group = new MultithreadEventLoopGroup();
            _bootstrap = new Bootstrap();
            _bootstrap
                .Group(_group)
                // 引导该NioDatagramChannel（无连接的）
                .Channel<SocketDatagramChannel>()
                // 设置SO_BROADCAST套接字选项
                .Option(ChannelOption.SoBroadcast, true)
                .Handler(new ActionChannelInitializer<DotNetty.Transport.Channels.IChannel>(channel =>
                {
                    channel.Pipeline.AddLast(new LogEventEncoder(address));
                }));
            _fileName = fileName;
        }

        public async Task Run()
        {
            // 绑定Channel
            DotNetty.Transport.Channels.IChannel ch = await _bootstrap.BindAsync(0);
            DateTime pointer = DateTime.MinValue;
            // 启动主处理循环
            for (; ; )
            {
                var lastWriteTime = File.GetLastWriteTime(_fileName);
                if (lastWriteTime > pointer)
                {
                    foreach (var line in File.ReadLines(_fileName))
                    {
                        // 对于每个日志条目，写入一个LogEvent到Channel中
                        await ch.WriteAndFlushAsync(new LogEvent(null, -1, _fileName, line));
                    }

                    pointer = lastWriteTime;
                }
                try
                {
                    // 休眠一秒，如果被中断，则退出循环；否则重新处理它
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                    break;
                }
            }
        }

        public async Task Stop()
        {
            await _group.ShutdownGracefullyAsync();
        }

        public static async Task Start(int port, string fileName)
        {
            // 创建并启动一个新的LogEventBroadcaster的实例
            LogEventBroadcaster broadcaster = new LogEventBroadcaster(new IPEndPoint(IPAddress.Broadcast, port), fileName);
            try
            {
                await broadcaster.Run();
            }
            finally
            {
                await broadcaster.Stop();
            }
        }
    }
}