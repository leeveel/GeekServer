using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Geek.Server.Udp
{
    public class LogEventEncoder : MessageToMessageEncoder<LogEvent>
    {

        private readonly EndPoint _remoteAddress;

        public LogEventEncoder(EndPoint remoteAddress)
        {
            _remoteAddress = remoteAddress;
        }

        protected override void Encode(IChannelHandlerContext context, LogEvent logEvent, List<object> output)
        {
            byte[] file = Encoding.UTF8.GetBytes(logEvent.GetLogfile());
            byte[] msg = Encoding.UTF8.GetBytes(logEvent.GetMsg());
            IByteBuffer buf = context.Allocator.Buffer(file.Length + msg.Length + 1);
            buf.WriteBytes(file);
            buf.WriteByte(LogEvent.SEPARATOR);
            buf.WriteBytes(msg);
            output.Add(new DatagramPacket(buf, _remoteAddress));
        }
    }
}