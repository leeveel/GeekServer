namespace Geek.Server
{
	public class SessionUtils
	{
		public static void WriteAndFlush(NetChannel ctx, Message msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			ctx.WriteAsync(msg);
		}

		public static void WriteAndFlush(NetChannel ctx, IMessage msg)
		{
			if (IsDisconnectChannel(ctx))
				return;
			Message smsg = Message.Create(msg.MsgId, msg.Serialize());
			ctx.WriteAsync(smsg);
		}

		public static void WriteAndFlush(NetChannel channel, int msgId, byte[] data)
		{
			if (msgId > 0 && data != null)
				WriteAndFlush(channel, new Message(msgId, data));
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