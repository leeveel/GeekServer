using Geek.Server.Proto;
using System;

namespace Geek.Server.Logic.Role
{
    /// <summary>
    /// 玩家基本信息，也是玩家的快照信息,初始化后不会释放
    /// </summary>
    public class RoleState : DBState
    {
        public string RoleName { get; set; }
        public long RoleId { get; set; }
        public int Level { get; set; } = 1;
        public int VipLevel { get; set; } = 1;
        public DateTime CreateTime { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime OfflineTime { get; set; }
    }

    /// <summary>
    /// RoleComp会作为玩家快照常驻内存
    /// </summary>
    public class RoleComp : StateComponent<RoleState>
    {

    }

}
