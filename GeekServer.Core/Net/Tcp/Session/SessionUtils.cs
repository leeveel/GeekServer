using DotNetty.Transport.Channels;
using Geek.Server;

public class SessionUtils
{

	public static void WriteAndFlush(IChannelHandlerContext ctx, NMessage msg)
	{
		if (IsDisconnectChannel(ctx))
			return;
		ctx.WriteAndFlushAsync(msg);
	}

	public static void WriteAndFlush(IChannelHandlerContext ctx, BaseMessage msg)
	{
		WriteAndFlush(ctx, msg.GetMsgId(), msg.Serialize());
	}

	public static void WriteAndFlush(IChannelHandlerContext ctx, int msgId, byte[] data)
	{
		if (msgId > 0 && data != null)
			WriteAndFlush(ctx, new NMessage() { MsgId = msgId, Data = data });
	}

	public static bool IsDisconnectChannel(IChannelHandlerContext ctx)
	{
		return ctx == null || ctx.Channel == null || !ctx.Channel.Active || !ctx.Channel.Open;
	}

	public static void CloseChannel(IChannelHandlerContext ctx)
	{
		if (ctx != null)
			ctx.CloseAsync();
	}

}
