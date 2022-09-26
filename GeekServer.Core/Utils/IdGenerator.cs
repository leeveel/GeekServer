
namespace Geek.Server
{

    /// <summary>
    /// 需要小于10000，因为10000以上作为服务器id了
    /// </summary>
    public enum IDModule
    {
        //单服/玩家不同即可
        Pet = 1001,
        Voyage = 1002,
        Rune = 1003,
        Robot = 1004,
        FightLog = 1005,
        EMail = 1006,
        EMailCMD = 1007,
        GodEquip = 1008,
        Delegate = 1009,
        Equip = 1010,
        Replay = 1011,
        ChargeOrder = 1012,

        //所有服不能相同的统一用serverID
        //所有服不同，但是由一个服分配
        IslandBattle = 2001,

        //日志id
        WorkerActor = 3002
    }

    /// <summary>
    /// ActorId
    ///     17   +   4  + 30 +  12   = 63
    ///     服务器id 类型 时间戳 自增
    ///         玩家
    ///         公会
    ///     服务器id * 1000 + 全局功能id
    ///         全局玩法
    ///         
    /// 装备id，机器人id
    ///     17   + 30     + 16 = 63
    ///     功能id 时间戳 自增
    ///         装备
    ///         机器人
    /// </summary>
    public static class IdGenerator
    {
        private const int SECOND_MASK = 0x3FFFFFFF;
        private const int MAX_GLOBAL_ID = 10000_0000;
        private const int MIN_SERVER_ID = 10000;
        private const int MAX_ACTOR_INCREASE = 0xFFF; // 4095
        private const int MAX_UNIQUE_INCREASE = 0xFFFF; // 65535

        private static long genSecond = 0L;
        private static long incrNum = 0L;

        //此时间决定可用id年限(最晚有效年限=34年+此时间)(可调整,早于开服时间就行)
        private readonly static DateTime utcTimeStart = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly object lockObj = new();

        public static int GetServerId(long id)
        {
            return (int)(id < MAX_GLOBAL_ID ? id / 1000 : id >> 46);
        }

        public static DateTime GetGenerateTime(long id, bool utc = false)
        {
            if (id < MAX_GLOBAL_ID)
            {
                throw new ArgumentException($"input is a global id:{id}");
            }

            var serverId = GetServerId(id);
            long seconds;
            if (serverId < MIN_SERVER_ID)
            {
                // IDModule unique_id
                seconds = (id >> 16) & SECOND_MASK;
            }
            else
            {
                seconds = (id >> 12) & SECOND_MASK;
            }

            var date = utcTimeStart.AddSeconds(seconds);
            return utc ? date : date.ToLocalTime();
        }

        public static ActorType GetActorType(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentException($"input id error:{id}");
            }

            if (id < MAX_GLOBAL_ID)
            {
                // 全局actor
                return (ActorType)(id % 1000);
            }

            return (ActorType)((id >> 42) & 0xF);
        }

        public static long GetActorID(int type, int serverId = 0)
        {
            return GetActorID((ActorType)type, serverId);
        }

        public static long GetActorID(ActorType type, int serverId = 0)
        {
            if (type == ActorType.Separator)
            {
                throw new ArgumentException($"input actor type error: {type}");
            }

            if (serverId < 0)
            {
                throw new ArgumentException($"serverId negtive when generate id {serverId}");
            }
            else if (serverId == 0)
            {
                serverId = Settings.ServerId;
            }

            if (type < ActorType.Separator)
            {
                return GetMultiActorID(type, serverId);
            }
            else
            {
                return GetGlobalActorID(type, serverId);
            }
        }

        public static long GetMultiActorIDBegin(ActorType type)
        {
            if (type >= ActorType.Separator)
            {
                throw new ArgumentException($"input actor type error: {type}");
            }
            var id = (long)Settings.ServerId << 46;
            id |= (long)type << 42;
            return id;
        }

        private static long GetGlobalActorID(ActorType type, int serverId)
        {
            return (long)(serverId * 1000 + type);
        }

        private static long GetMultiActorID(ActorType type, int serverId)
        {
            long second = (long)(DateTime.UtcNow - utcTimeStart).TotalSeconds;
            lock (lockObj)
            {
                if (second > genSecond)
                {
                    genSecond = second;
                    incrNum = 0L;
                }
                else if (incrNum >= MAX_ACTOR_INCREASE)
                {
                    ++genSecond;
                    incrNum = 0L;
                }
                else
                {
                    ++incrNum;
                }
            }

            var id = (long)serverId << 46; // serverId 17位 支持10000~99999
            id |= (long)type << 42; // 多actor类型 4位 支持0~15
            id |= genSecond << 12; // 时间戳 30位
            id |= incrNum; // 自增 12位
            return id;
        }

        public static long GetUniqueId(IDModule module)
        {
            long second = (long)(DateTime.UtcNow - utcTimeStart).TotalSeconds;
            lock (lockObj)
            {
                if (second > genSecond)
                {
                    genSecond = second;
                    incrNum = 0L;
                }
                else if (incrNum >= MAX_UNIQUE_INCREASE)
                {
                    ++genSecond;
                    incrNum = 0L;
                }
                else
                {
                    ++incrNum;
                }
            }
            var id = (long)module << 46; // 模块id 17位 支持 0~9999
            id |= genSecond << 16; // 时间戳 30位
            id |= incrNum; // 自增 16位
            return id;
        }
    }


}

