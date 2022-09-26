
public static class TcpHandlerExtensions
{
    public static void WriteWithErrCode(this BaseTcpHandler handler, MSG msg)
    {
        var Msg = handler.Msg;
        if (Msg != null)
        {
            msg.msg.UniId = Msg.UniId;
            handler.WriteAsync(new NMessage(msg.msg));
        }
        handler.NotifyErrorCode(msg.Info);
    }

    public static void NotifyErrorCode(this BaseTcpHandler handler, ErrInfo errInfo)
    {
        var Msg = handler.Msg;
        ResErrorCode res = new ResErrorCode
        {
            UniId = Msg.UniId,  //写入req中的UniId
            ErrCode = (int)errInfo.Code,
            Desc = errInfo.Desc
        };
        NMessage msg = new NMessage(res);
        handler.WriteAsync(msg);
    }
}