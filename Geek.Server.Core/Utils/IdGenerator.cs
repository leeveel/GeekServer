
using Geek.Server.Core.Actors;

namespace Geek.Server.Core.Utils
{

    /// <summary>
    /// 需要小于1000，因为1000以上作为服务器id了
    /// </summary>
    public enum IDModule
    {
        MIN = 0,
        //单服/玩家不同即可
        Pet = 101,
        Equip = 102,
        WorkerActor = 103,
        MAX = 999
    }

    /// <summary>
    /// ActorId
    ///     14   +   7  + 30 +  12   = 63
    ///     服务器id 类型 时间戳 自增
    ///         玩家
    ///         公会
    ///     服务器id * 1000 + 全局功能id
    ///         全局玩法
    /// </summary>
    public static class IdGenerator
    {
        private const int SECOND_MASK = 0x3FFFFFFF;
        private const int MAX_GLOBAL_ID = 10000_000;
        public const int MIN_SERVER_ID = 1000;
        public const int MAX_SERVER_ID = 9999;
        private const int MAX_ACTOR_INCREASE = 0xFFF; // 4095
        private const int MAX_UNIQUE_INCREASE = 0x7FFFF; //524287

        private static long genSecond = 0L;
        private static long incrNum = 0L;

        //此时间决定可用id年限(最晚有效年限=34年+此时间)(可调整,早于开服时间就行)
        private readonly static DateTime utcTimeStart = new(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly object lockObj = new();

        public static int GetServerId(long id)
        {
            return (int)(id < MAX_GLOBAL_ID ? id / 1000 : id >> SERVERID_OR_MODULEID_MASK);
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
                seconds = (id >> MODULEID_TIMESTAMP_MASK) & SECOND_MASK;
            }
            else
            {
                seconds = (id >> TIMESTAMP_MASK) & SECOND_MASK;
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

            return (ActorType)((id >> ACTORTYPE_MASK) & 0xF);
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
            var id = (long)Settings.ServerId << SERVERID_OR_MODULEID_MASK;
            id |= (long)type << ACTORTYPE_MASK;
            return id;
        }

        private static long GetGlobalActorID(ActorType type, int serverId)
        {
            return (long)(serverId * 1000 + type);
        }

        const int SERVERID_OR_MODULEID_MASK = 49;   //49+14=63
        const int ACTORTYPE_MASK = 42;  //42+7 = 49
        const int TIMESTAMP_MASK = 12;   //12+30 =42
        const int MODULEID_TIMESTAMP_MASK = 19;       //19+30 =42
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

            var id = (long)serverId << SERVERID_OR_MODULEID_MASK; // serverId-14位, 支持1000~9999
            id |= (long)type << ACTORTYPE_MASK; // 多actor类型-7位, 支持0~127
            id |= genSecond << TIMESTAMP_MASK; // 时间戳-30位, 支持34年
            id |= incrNum; // 自增-12位, 每秒4096个
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
            var id = (long)module << SERVERID_OR_MODULEID_MASK; // 模块id 14位 支持 0~9999
            id |= genSecond << MODULEID_TIMESTAMP_MASK; // 时间戳 30位
            id |= incrNum; // 自增 19位
            return id;
        }
    }

}

