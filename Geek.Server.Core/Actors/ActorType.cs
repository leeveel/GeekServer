namespace Geek.Server.Core.Actors
{

    /// <summary>
    /// 每个服存在多个实例的（如玩家和公会）需要小于Separator
    /// 最大id应当小于999
    /// Id一旦定义了不应该修改
    /// </summary>
    public enum ActorType
    {
        //ID全服唯一类型
        None,
        Role, // 角色
        Guild, //公会 

        Separator = 128, /*分割线(勿调整,勿用于业务逻辑)*/

        //固定ID类型Actor
        Server = 129,

        Max = 999,
    }

    /// <summary>
    /// 供ActorLimit检测调用关系
    /// </summary>
    public enum ActorTypeLevel
    {
        Role = 1,
        Guild, 
        Server,
    }

}
