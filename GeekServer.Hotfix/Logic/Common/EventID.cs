
namespace Geek.Server
{
    public enum EventID
    {
        OnDisconnected = 1000, //断线
        OnMsgReceived = 1001,  //收到消息

        #region role event
        //玩家事件
        RoleLevelUp = 10000, //玩家等级提升
        RoleVipChange, //玩家vip改变
        OnRoleOnline, //玩家上线
        OnRoleOffline, //玩家下线
        #endregion

        //分割线(勿用于业务逻辑)
        Separator = 50000,

        #region server event
        //服务器事件
        WorldLevelChange, //世界等级改变
        ReloadBean, //服务器开启时重载配置表
        #endregion


    }
}
