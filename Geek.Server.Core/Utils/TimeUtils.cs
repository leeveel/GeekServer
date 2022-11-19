namespace Geek.Server.Core.Utils
{
    public static class TimeUtils
    {

        /// <summary>
        /// 获取跨过了几天
        /// </summary>
        public static int GetCrossDays(DateTime begin, int hour = 0)
        {
            return GetCrossDays(begin, DateTime.Now, hour);
        }

        /// <summary>
        /// 获取跨过了几天
        /// </summary>
        public static int GetCrossDays(DateTime begin, DateTime after, int hour = 0)
        {
            int days = (int)(after.Date - begin.Date).TotalDays + 1;
            if (begin.Hour < hour)
                days++;
            if (after.Hour < hour)
                days--;
            return days;
        }

        public static bool IsNowSameWeek(long start)
        {
            return IsNowSameWeek(new DateTime(start));
        }

        public static bool IsNowSameWeek(DateTime start)
        {
            return IsSameWeek(start, DateTime.Now);
        }

        /// <summary>
        /// 是否同一周
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static bool IsSameWeek(DateTime start, DateTime end)
        {
            // 让start是较早的时间
            if (start > end)
            {
                DateTime temp = start;
                start = end;
                end = temp;
            }

            int dayOfWeek = (int)start.DayOfWeek;
            if (dayOfWeek == (int)DayOfWeek.Sunday) dayOfWeek = 7;
            // 获取较早时间所在星期的星期天的0点
            var startsWeekLastDate = start.AddDays(7 - dayOfWeek).Date;
            // 判断end是否在start所在周
            return startsWeekLastDate >= end.Date;
        }

        public static DateTime GetDayOfWeekTime(DateTime t, DayOfWeek d)
        {
            int dd = (int)d;
            if (dd == 0) dd = 7;
            var dayOfWeek = (int)t.DayOfWeek;
            if (dayOfWeek == 0) dayOfWeek = 7;
            return t.AddDays(dd - dayOfWeek).Date;
        }

        public static DateTime GetDayOfWeekTime(DayOfWeek d)
        {
            return GetDayOfWeekTime(DateTime.Now, d);
        }

        public static int GetChinaDayOfWeek(DayOfWeek d)
        {
            int dayOfWeek = (int)d;
            if (dayOfWeek == 0) dayOfWeek = 7;
            return dayOfWeek;
        }

        public static int GetChinaDayOfWeek()
        {
            return GetChinaDayOfWeek(DateTime.Now.DayOfWeek);
        }
    }
}
