using System;

namespace Geek.Server
{
    public class IdGenerator
    {
        static long cacheSecond = 0L;
        static long genSecond = 0L;
        static long increaseNumber = 0L;

        readonly static long gapSecond = (long)TimeSpan.FromDays(365.2422f * 35).TotalSeconds;

        //此时间决定可用id年限(最晚有效年限=34年+此时间)(需早于开服时间)
        readonly static DateTime utcTimeStart = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        static readonly object lockObj = new object();
        /// <summary>
        /// 生成全局唯一id，如roleId
        /// <para>服务器之间不能相同的统一用serverId</para>
        /// </summary>
        public static long GetUniqueId(int serverOrModuleId)
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
            long res = (long)(serverOrModuleId) << 47;//(63 - 16);//serverId 前16位[最大65535]
            res |= 0x00007FFFFFFFFFFF & (second << 17);//(63-16-17)时间戳用中间30位(可表示1073741823秒≈34年=2020+34=2054年)
            return res | increaseNum;
        }

        /// <summary>
        /// 获取id生成时间,精确到秒
        /// </summary>
        public static DateTime GetGenerateTime(long id, bool utc = false)
        {
            id &= 0x00007FFFFFFFFFFF;//去掉高16位(serverId)
            id >>= 17;//去掉自增低17位
            var date = utcTimeStart.AddSeconds(id);

            //扩容判断
            while ((date - utcTimeStart).TotalSeconds > gapSecond)
                date = date.AddSeconds(-gapSecond);

            if (!utc)
                return date.ToLocalTime();
            return date;
        }

        /// <summary>
        /// 通过唯一id获取生成所在服务器id
        /// </summary>
        public static int GetServerId(long id)
        {
            return (int)(id >> 47);
        }
    }
}