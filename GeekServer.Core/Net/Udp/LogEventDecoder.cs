using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server.Udp
{
    public class LogEventDecoder : MessageToMessageDecoder<DatagramPacket>
    {

        protected override void Decode(IChannelHandlerContext context, DatagramPacket datagramPacket, List<object> output)
        {
            // 获取对DatagramPacket中的数据（ByteBuf）的引用
            IByteBuffer data = datagramPacket.Content;
            // 获取该SEPARATOR的索引
            int idx = data.IndexOf(0, data.ReadableBytes, LogEvent.SEPARATOR);
            // 提取文件名
            string filename = data.Slice(0, idx).ToString(Encoding.UTF8);
            // 提取日志消息
            string logMsg = data.Slice(idx + 1, data.ReadableBytes).ToString(Encoding.UTF8);
            // 构建一个新的LogEvent对象，并且将它添加到（已经解码的消息的）列表中
            LogEvent evt = new LogEvent(datagramPacket.Sender, DateTime.Now.Ticks, filename, logMsg);
            output.Add(evt);
        }

    }
}
