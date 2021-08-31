using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server.Net.Udp
{
    public class LogEventHandler : SimpleChannelInboundHandler<LogEvent>
    {

        protected override void ChannelRead0(IChannelHandlerContext ctx, LogEvent msg)
        {
            // 创建StringBuilder，并且构建输出的字符串
            StringBuilder builder = new StringBuilder()
                .Append(new DateTime(msg.GetReceived()))
            .Append(" [")
            .Append(msg.GetSource().ToString())
            .Append("] [")
            .Append(msg.GetLogfile())
            .Append("] : ")
            .Append(msg.GetMsg());
            // 打印LogEvent的数据
            Console.WriteLine(builder.ToString());
        }
    }
}
