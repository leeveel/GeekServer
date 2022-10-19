
public static class NetChannelExtensions
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public static void WriteAsync(this NetChannel channel, Message msg, int uniId, StateCode code = StateCode.Success, string desc = "")
    {
        if (msg != null)
        {
            msg.UniId = uniId;
            channel.WriteAsync(new NMessage(msg));
        }
        if (uniId > 0)
        {
            ResErrorCode res = new ResErrorCode
            {
                UniId = uniId,
                ErrCode = (int)code,
                Desc = desc
            };
            //Log.Error(channel.GetSessionId() + " + " + msg.GetType().FullName + " + " + res.UniId);
            channel.WriteAsync(new NMessage(res));
        }
    }
}