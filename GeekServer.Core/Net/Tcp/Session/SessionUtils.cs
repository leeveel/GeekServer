using DotNetty.Transport.Channels;

namespace Geek.Server
{
	public class SessionUtils
	{
		public static void WriteAndFlush(IChannelHandlerContext ctx, NMessage msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			ctx.WriteAndFlushAsync(msg);
		}

		public static void WriteAndFlush(IChannelHandlerContext ctx, IMessage msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			NMessage smsg = NMessage.Create(msg.GetMsgId(), msg.Serialize());
			ctx.WriteAndFlushAsync(smsg);
		}

		public static void WriteAndFlush(IChannel channel, NMessage msg)
		{
			if (IsDisconnectChannel(channel))
				return;
			channel.WriteAndFlushAsync(msg);
		}

		public static void WriteAndFlush(IChannel channel, IMessage msg)
		{
			WriteAndFlush(channel, msg.GetMsgId(), msg.Serialize());
		}

		public static void WriteAndFlush(IChannel channel, int msgId, byte[] data)
		{
			if (msgId > 0 && data != null)
				WriteAndFlush(channel, new NMessage() { MsgId = msgId, Data = data });
		}

		public static bool IsDisconnectChannel(IChannel channel)
		{
			return channel == null || !channel.Active || !channel.Open;
		}

		public static bool IsDisconnectChannel(IChannelHandlerContext ctx)
		{
			// copy一份避免多线程问题
			IChannelHandlerContext tmp = ctx;
			return tmp == null || tmp.Channel == null || !tmp.Channel.Active || !tmp.Channel.Open;
		}

		public static void CloseChannel(IChannelHandlerContext channel)
		{
			if (channel != null)
				channel.CloseAsync();
		}

		public static void CloseChannel(IChannel channel)
		{
			if (channel != null)
				channel.CloseAsync();
		}
	}
}