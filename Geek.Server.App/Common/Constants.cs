namespace Geek.Server.App.Common
{

    /// <summary>
    /// 服务器错误码
    /// </summary>
    public enum StateCode
    {
        Success = 0,
        ConfigErr = 400, //配置表错误
        ParamErr, //客户端传递参数错误
        CostNotEnough, //消耗不足

        //Login
        AccountCannotBeNull,
        UnknownPlatform,

        Notice = 100000, //正常通知
        FuncNotOpen, //功能未开启，主消息屏蔽
        Other //其他
    }
}
