
using Geek.Server.App.Common;
using Geek.Server.Core.Net;

namespace Server.Logic.Common.Handler;

public static class NetChannelExtensions
{
    public static void Write(this NetChannel channel, Message msg, int uniId, StateCode code = StateCode.Success, string desc = "")
    {
        if (msg != null)
        {
            msg.UniId = uniId;
            channel.Write(msg);
        }
        if (uniId > 0)
        {
            ResErrorCode res = new ResErrorCode
            {
                UniId = uniId,
                ErrCode = (int)code,
                Desc = desc
            };
            channel.Write(res);
        }
    }
}