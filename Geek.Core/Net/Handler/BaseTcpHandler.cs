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
using Geek.Core.Net.Message;
using DotNetty.Transport.Channels;
using System.Threading.Tasks;

namespace Geek.Core.Net.Handler
{
    public abstract class BaseTcpHandler
    {
        /// <summary>
        ///  连接
        /// </summary>
        public IChannelHandlerContext Ctx { get; set; }

        /// <summary>
        /// 从Decoder中转化出来的时间
        /// </summary>
        public long Time;

        /// <summary>
        /// 消息id
        /// </summary>
        public int MsgId
        {
            get
            {
                if (Msg == null)
                    return 0;
                else
                    return Msg.GetMsgId();
            }
        }

        /// <summary>
        /// 消息体
        /// </summary>
        public IMessage Msg { get; set; }

        public abstract Task ActionAsync();

        protected virtual void WriteAndFlush(SMessage msg)
        {
            ChannelUtils.SendToClient(Ctx, msg);
        }

        protected virtual void WriteAndFlush(int msgId, byte[] data)
        {
            if (msgId > 0 && data != null)
            {
                SMessage msg = new SMessage(msgId, data);
                ChannelUtils.SendToClient(Ctx, msg);
            }
        }
    }
}
