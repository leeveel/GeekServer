using System;

namespace Geek.Server
{

    /// <summary>
    /// 范围约定[为配合ActorID进行ID规则运算,请严格遵循该规则]
    ///            [此ID规则完全足够使用,当然你可以制定你自己规则]
    ///            
    /// Actor类型有三种:
    /// 1.ID全服唯一类型 [0,127]   [生成的ID合服之后也要保持唯一,这种理论上不会太多(角色,公会,一般为数据库的key),预留了128种类型]            
    /// 2.ID仅需要单服唯一的Actor类型 [GeekServer为了方便统一使用了0-127这个范围,可自己根据情况调整]
    /// 3.ID固定,每个服只会有一个的Actor类型 [129, 999] [这种ActorID会通过固定的规则生成serverId*1000 + actortype]
    /// 非Actor类型的ID可以使用IdGenerator
    /// </summary>
    public enum ActorType
    {
        //ID全服唯一类型
        Role = 1,
        Guild,


        Separator = 128,   /*分割线(勿调整,勿用于业务逻辑)*/

        //单服仅有一个类型(SingletonActor)
        Server,
        Login,
        Rank,

        Max =1000,
    }

    /// <summary>
    /// 所有ActorID应使用此类去生成
    /// </summary>
    public static class ActorID
    {

        public static long GetID(ActorType actorType, int serverId = 0)
        {
            if (serverId <= 0)
                serverId = Settings.Ins.ServerId;
            if (actorType > ActorType.Separator)
                return serverId * 1000 + (int)actorType;
            throw new Exception($"此接口仅用于获取当前服固定ID类型的ActorID:{actorType}");
        }

        public static long NewID(ActorType actorType, int serverId=0)
        {
            if (serverId <= 0)
                serverId = Settings.Ins.ServerId;
            if (actorType < ActorType.Separator)
                return GetActorId(serverId, (int)actorType);
            else
                throw new Exception($"此接口不能用于获取当前服固定ID类型的ActorID:{actorType}");
        }

        public static ActorType GetActorType(long actorId)
        {
            //serverid[1-16383]*1000=16383000, actortype[1-999]
            if (actorId < 99999999)
                return (ActorType)(actorId % 1000);
            long temp = actorId >> 42;   //剩下高22位
            temp &= 0x7F;  //保留低7位
            return (ActorType)temp;
        }

        /// <summary>
        /// ID规则
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public static int ID_RULE(long actorId)
        {
            return (int)GetActorType(actorId);
        }

        /// <summary>
        /// 通过唯一id获取生成所在服务器id
        /// </summary>
        public static int GetServerId(long id)
        {
            return (int)(id >> 49); //(63-14)
        }

        /// <summary>
        /// 获取id生成时间,精确到秒
        /// </summary>
        public static DateTime GetGenerateTime(long id, bool utc = false)
        {
            id &= 0x000007FFFFFFFFFF;//去掉高1+16+5位(符号位+serverId+actorType)
            id >>= 12;//去掉自增低17位
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

        readonly static long gapSecond = (long)TimeSpan.FromDays(365.2422f * 35).TotalSeconds;

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
        public static long GetActorId(int serverId, int actorType)
        {
            long second = (long)(DateTime.UtcNow - utcTimeStart).TotalSeconds;
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
                    //如果越界时间往后推35年,扩容数量(9999-2020)/35≈227,  (9999=DateTime.MaxValue.Year)
                    //自增12位，每秒可生成4096*227≈933770个
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
            long res = (long)serverId << 49;//(63-14) serverId 前14位[最大16383]
            res |= (long)actorType << 42; //(63-14-7) actorType[最大127]
            res |= 0x000007FFFFFFFFFF & (second << 12);//(63-14-7-12)时间戳用中间30位(可表示1073741823秒≈34年=2020+34=2054年)
            return res | increaseNum;
        }
        #endregion

    }
}
