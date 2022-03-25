using System;

/// <summary>
/// 时间戳工具类
/// </summary>
public static class TimeUtils
{
    private static readonly DateTime epochLocal = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
    private static readonly DateTime epochUtc = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Utc);

    public static long CurrentTimeMillis(bool utc = false)
    {
        return TimeMillis(DateTime.Now, utc);
    }

    /// <summary>
    /// 某个时间的毫秒数
    /// </summary>
    /// <param name="time"></param>
    /// <param name="utc"></param>
    /// <returns></returns>
    public static long TimeMillis(DateTime time, bool utc = false)
    {
        if (utc)
            return (long)(time - epochUtc).TotalMilliseconds;
        return (long)(time - epochLocal).TotalMilliseconds;
    }

    /// <summary>
    /// 毫秒转时间
    /// </summary>
    /// <param name="time"></param>
    /// <param name="utc"></param>
    /// <returns></returns>
    public static DateTime MillisToDateTime(long time, bool utc = false)
    {
        if (utc)
            return epochUtc.AddMilliseconds(time);
        return epochLocal.AddMilliseconds(time);
    }

    public static bool isNowSameWeek(long start)
    {
        return IsSameWeek(new DateTime(start), DateTime.Now);
    }
    public static bool IsSameWeek(DateTime start, DateTime end)
    {
        var interval = end - start;
        var totalDays = interval.TotalDays;
        int dayWeek = (int)end.DayOfWeek;
        if (dayWeek == 0) dayWeek = 7;
        if (totalDays >= 7 || totalDays >= dayWeek)
            return false;
        return true;
    }

    public static bool IsSameDay(this DateTime start, DateTime end)
    {
        return start.ToString("yyyy-MM-dd").Equals(end.ToString("yyyy-MM-dd"));
    }

}