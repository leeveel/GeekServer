using Geek.Server.App.Net.Session;

public static class SessionExtensions
{
    public static void WriteAsync(this Session session, Message msg, int uniId, StateCode code = StateCode.Success, string desc = "")
    {
        if (msg != null)
        {
            msg.UniId = uniId;
            session.WriteAsync(msg);
        }
        if (uniId > 0)
        {
            var res = new ResErrorCode
            {
                UniId = uniId,
                ErrCode = (int)code,
                Desc = desc
            };
            session.WriteAsync(res);
        }
    }
}