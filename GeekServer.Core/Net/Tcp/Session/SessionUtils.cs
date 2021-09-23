using DotNetty.Transport.Channels;
using Geek.Server;

public class SessionUtils
{

	public static void WriteAndFlush(IChannel channel, NMessage msg)
	{
		if (IsDisconnectChannel(channel))
			return;
		channel.WriteAndFlushAsync(msg);
	}

	public static void WriteAndFlush(IChannel channel, BaseMessage msg)
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

	public static void CloseChannel(IChannelHandlerContext channel)
	{
		if (channel != null)
			channel.CloseAsync();
	}

}
