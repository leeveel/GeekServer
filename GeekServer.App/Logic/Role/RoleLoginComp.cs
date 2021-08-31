using System;

namespace Geek.Server.Logic.Role
{

    public class RoleInfoState : DBState
    {
        public string RoleName { get; set; }
        public long RoleId { get; set; }
        public int Level { get; set; } = 1;
        public int VipLevel { get; set; } = 1;
        public DateTime CreateTime { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime OfflineTime { get; set; }
    }

    public class RoleLoginComp : StateComponent<RoleInfoState> { }

}
