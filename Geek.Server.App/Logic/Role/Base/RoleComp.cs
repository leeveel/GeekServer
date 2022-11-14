
using MessagePack;

namespace Geek.Server.Role
{

    [Comp(ActorType.Role)]
    public class RoleComp : StateComp<RoleState>
    {

    }

    public class RoleState : CacheState
    {
        public long RoleId => Id;
        public string RoleName;
        public int Level = 1;
        public int VipLevel = 1;
        public DateTime CreateTime;
        public DateTime LoginTime;
        public DateTime OfflineTime;
    }
}
