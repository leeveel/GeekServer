/****************************************************************************
Copyright (c) Geek

https://github.com/leeveel/GeekServer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/
using System;

/// <summary>
/// 时间戳工具类
/// </summary>
public class TimeUtils
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

}