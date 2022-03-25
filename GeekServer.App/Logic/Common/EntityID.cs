using System;

namespace Geek.Server
{
    /// <summary>
    /// 范围约定[为配合ActorID进行ID规则运算,请严格遵循该规则]
    ///            [此ID规则完全足够使用,当然你可以制定你自己规则]
    ///
    /// Actor类型有三种:
    /// 1.ID全服唯一类型 [1,15]   [生成的ID合服之后也要保持唯一,这种理论上不会太多(角色,公会,一般为数据库的key),预留了15种类型]
    /// 2.ID仅需要单服唯一的Actor类型 [GeekServer为了方便统一使用了1-15这个范围,可自己根据情况调整]
    /// 3.ID固定,每个服只会有一个的Actor类型 [17, 999] [这种ActorID会通过固定的规则生成serverId*1000 + actortype]
    /// 非Actor类型的ID可以使用IdGenerator
    /// </summary>
    public enum EntityType
    {
        //ID全服唯一类型
        None,
        Role = 1,
        Guild,

        Separator = 16, /*分割线(勿调整,勿用于业务逻辑)*/

        //固定ID类型Actor
        Server,
        Center,
        Login,
    }

    /// <summary>
    /// 所有ActorID应使用此类去生成
    /// </summary>
    public static class EntityID
    {
        public static long NewID(EntityType entityType, int serverId = 0)
        {
            if (serverId <= 0)
                serverId = Settings.Ins.ServerId;
            if (entityType < EntityType.Separator)
                return GetEntityID(serverId, (int)entityType);
            else
                return serverId * 1000L + (int)entityType;
        }

        public static long GetID(EntityType entityType, int serverId = 0)
        {
            return NewID(entityType, serverId);
        }

        public static EntityType GetEntityType(long entityId)
        {
            if (entityId < 10000_0000)
                return (EntityType)(entityId % 1000);
            long temp = entityId >> 59;//高4位
            return (EntityType)temp;
        }

        /// <summary>
        /// ID规则
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static int GetEntityTypeFromID(long entityId)
        {
            return (int)GetEntityType(entityId);
        }

        public static long GetEntityIdFromType(int entityType)
        {
            return GetID((EntityType) entityType);
        }

        /// <summary>
        /// 通过唯一id获取生成所在服务器id
        /// </summary>
        public static int GetServerId(long id)
        {
            id &= 0x07FFFFFFFFFFFFFF; //高符号位+actorType
            id >>= 42; //时间30+自增12位
            return (int) id;
        }

        /// <summary>
        /// 获取id生成时间,精确到秒
        /// </summary>
        public static DateTime GetGenerateTime(long id, bool utc = false)
        {
            id &= 0x000003FFFFFFFFFF; //去掉高1+4+17位(符号位+actorType+serverId)
            id >>= 12; //去掉自增低12位
            var date = utcTimeStart.AddSeconds(id);
            //扩容判断
            while ((date - utcTimeStart).TotalSeconds > gapSecond)
                date = date.AddSeconds(-gapSecond);

            if (!utc)
                return date.ToLocalTime();
            return date;
        }


        #region gen id

        static long cacheSecond = 0L;
        static long genSecond = 0L;
        static long increaseNumber = 0L;

        readonly static long gapSecond = (long) TimeSpan.FromDays(365.2422f * 35).TotalSeconds;

        //此时间决定可用id年限(最晚有效年限=34年+此时间)(可调整,早于开服时间就行)
        //即:2020+34=2054年后的ID有可能和之前的ID重复(几乎可以忽略)
        readonly static DateTime utcTimeStart = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static readonly object lockObj = new object();

        /// <summary>
        /// 全服唯一ID
        /// </summary>
        /// <param name="serverId">服务器ID</param>
        /// <param name="actorType">Actor类型</param>
        /// <returns></returns>
        public static long GetEntityID(int serverId, int actorType)
        {
            long second = (long) (DateTime.UtcNow - utcTimeStart).TotalSeconds;
            long increaseNum = 0L;
            lock (lockObj)
            {
                if (second != cacheSecond)
                {
                    cacheSecond = second;
                    genSecond = second;
                    increaseNumber = 0L;
                }
                else if (increaseNumber >= 0x0FFF)
                {
                    //如果越界时间往后推35年,扩容数量(9999-2020)/35≈227,  (9999=DateTime.MaxValue.Year)
                    //自增12位，每秒可生成4096*227≈929792个
                    second = genSecond += gapSecond;
                    increaseNumber = 0L;
                }
                else
                {
                    second = genSecond;
                    increaseNum = ++increaseNumber;
                }
            }

            //保证id为正数，最高位不用
            long res = (long) actorType << 59; //(63-4) actorType[最大15]
            res |= (long) serverId << 42; //(63-4-17) serverId 前17位[最大131072(取10000到99999)]
            res |= second << 12; //(63-4-17-30)时间戳用中间30位(可表示1073741823秒≈34年=2020+34=2054年)
            res |= increaseNum; //自增
            return res;
        }

        public static long GetGlobalUniqueId()
        {
            return GetUniqueId(Settings.Ins.ServerId);
        }

        public static long GetUniqueId(IDModule module)
        {
            return GetUniqueId((int)module);
        }

        /// <summary>
        /// 生成全局唯一id，如roleId
        /// <para>服务器之间不能相同的统一用serverId</para>
        /// </summary>
        public static long GetUniqueId(int module)
        {
            long second = (long) (DateTime.UtcNow - utcTimeStart).TotalSeconds;
            long increaseNum = 0L;
            lock (lockObj)
            {
                if (second != cacheSecond)
                {
                    cacheSecond = second;
                    genSecond = second;
                    increaseNumber = 0L;
                }
                else if (increaseNumber >= 0x1FFFF)
                {
                    //自增17位，每秒可生成131071个*扩容倍数≈26214200个
                    //如果越界时间往后推35年,扩容数量9999-2999/35≈200
                    second = genSecond += gapSecond;
                    increaseNumber = 0L;
                }
                else
                {
                    second = genSecond;
                    increaseNum = ++increaseNumber;
                }
            }

            //保证id为正数，最高位不用
            long res = (long) (module) << 47; //(63 - 16);//serverId 前16位[最大65535]
            res |= 0x00007FFFFFFFFFFF & (second << 17); //(63-16-17)时间戳用中间30位(可表示1073741823秒≈34年=2020+34=2054年)
            return res | increaseNum;
        }

        public static int GetModuleId(long id)
        {
            return (int)(id >> 47);
        }
        #endregion
    }
}

public enum IDModule
{
    //单服/玩家不同即可
    Pet = 1001,
    Robot = 1002,
    EMail = 1003,
    Equip = 1004,
}