
namespace Geek.Server
{
    public enum EventID
    {

        #region inner event
        OnDisconnected = InnerEventID.OnDisconnected, //10000 断线
        OnMsgReceived = InnerEventID.OnMsgReceived,  //10001 收到消息
        #endregion

        #region role event
        //玩家事件
        RoleLevelUp = 20001, //玩家等级提升
        RoleVipChange, //玩家vip改变
        OnRoleOnline, //玩家上线
        OnRoleOffline, //玩家下线
        #endregion

        //分割线(勿用于业务逻辑)
        Separator = 30000,

        #region global event
        //服务器事件
        HotfixEnd,
        WorldLevelChange, //世界等级改变
        ReloadBean, //服务器开启时重载配置表
        #endregion
    }
}