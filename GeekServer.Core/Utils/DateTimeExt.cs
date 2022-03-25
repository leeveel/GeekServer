using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utils
{
    public static class DateTimeExt
    {
        public static int GetDaysFrom(this DateTime now, DateTime dt)
        {
            return (int)(now.Date - dt).TotalDays;
        }

        public static int GetDaysFromDefault(this DateTime now)
        {
            return now.GetDaysFrom(new DateTime(1970, 1, 1).Date);
        }

        public static int GetTimeStampDefault(this DateTime now, DateTime dt)
        {
            return (int)(now - dt).TotalSeconds;
        }

        public static int GetTimeStampDefault(this DateTime now)
        {
            return GetTimeStampDefault(now, new DateTime(1970, 1, 1));
        }
    }
}
