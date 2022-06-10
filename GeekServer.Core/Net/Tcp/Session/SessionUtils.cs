namespace Geek.Server
{
	public class SessionUtils
	{
		public static void WriteAndFlush(NetChannel ctx, NMessage msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			ctx.WriteAsync(msg);
		}

		public static void WriteAndFlush(NetChannel ctx, IMessage msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			NMessage smsg = NMessage.Create(msg.MsgId, msg.Serialize());
			ctx.WriteAsync(smsg);
		}

		public static void WriteAndFlush(NetChannel channel, int msgId, byte[] data)
		{
			if (msgId > 0 && data != null)
				WriteAndFlush(channel, new NMessage(msgId, data));
		}

		public static bool IsDisconnectChannel(NetChannel channel)
		{
			return channel == null || channel.Context == null;
		}

		public static void CloseChannel(NetChannel channel)
		{
			if (channel != null)
				channel.Abort();
		}
	}
}