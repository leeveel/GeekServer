namespace System
{ 
    public static class DateTimeExtensions
    {
        public static int GetDaysFrom(this DateTime now, DateTime dt)
        {
            return (int)(now.Date - dt).TotalDays;
        }

        public static int GetDaysFromDefault(this DateTime now)
        {
            return now.GetDaysFrom(new DateTime(1970, 1, 1).Date);
        }
    }
}