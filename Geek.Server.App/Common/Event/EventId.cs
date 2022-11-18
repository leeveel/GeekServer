namespace Geek.Server.App.Common.Event
{
    public enum EventID
    {
        #region role event
        //玩家事件
        SessionRemove = 1000,
        RoleLevelUp = 1001, //玩家等级提升
        RoleVipChange, //玩家vip改变
        OnRoleOnline, //玩家上线
        OnRoleOffline, //玩家下线

        GotNewPet, // 解锁用
        #endregion

        /// <summary>
        /// 玩家事件分割点
        /// </summary>
        RoleSeparator = 8000,

        #region server event
        //服务器事件
        WorldLevelChange, //世界等级改变
        #endregion
    }
}
