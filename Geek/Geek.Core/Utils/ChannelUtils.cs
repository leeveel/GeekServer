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

public class ChannelUtils
{

	/// <summary>
	/// 发送消息到客户端
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="msg"></param>
    public static void SendToClient(IChannelHandlerContext ctx, SMessage msg)
    {
		if (IsDisconnectChannel(ctx))
			return;
		ctx.WriteAndFlushAsync(msg);
	}

	public static void SendToClient(IChannelHandlerContext ctx, IMessage msg)
	{
		if (IsDisconnectChannel(ctx))
			return;
		SMessage smsg = new SMessage(msg.GetMsgId(), msg.Serialize());
		ctx.WriteAndFlushAsync(smsg);
	}

	/// <summary>
	/// 关闭通道
	/// </summary>
	/// <param name="ctx"></param>
	public static void CloseChannel(IChannelHandlerContext ctx)
	{
		if (ctx != null)
			ctx.CloseAsync();
	}

	/// <summary>
	/// 判断此Netty连接是否无效
	/// </summary>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static bool IsDisconnectChannel(IChannelHandlerContext ctx)
	{
		// copy一份避免多线程问题
		IChannelHandlerContext tmp = ctx;
		return tmp == null || tmp.Channel == null || !tmp.Channel.Active || !tmp.Channel.Open;
	}

}
