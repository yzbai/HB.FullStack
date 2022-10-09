
using System.Globalization;

namespace System
{
    public static class TimeUtil
    {
        private static readonly ChineseLunisolarCalendar _cc = new ChineseLunisolarCalendar();

        //public static UtcNowTicks UtcNowTicks => UtcNowTicks.Instance;

        public static long UtcNowTicks => DateTimeOffset.UtcNow.UtcTicks;

        public static long Timestamp => DateTimeOffset.UtcNow.UtcTicks;

        public static long UtcNowUnixTimeSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public static long UtcNowUnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTime LocalNow => DateTime.Now;

        /// <summary>
        /// 把公历转换为人们口中的农历
        /// 机器农历里闰月是继续编下取的，可以有13个月
        /// </summary>
        public static (int nYear, int nMonth, int nDay, bool isMonthLeap) FromGongliToNongli(int year, int month, int day)
        {
            DateTime dt = new DateTime(year, month, day);

            int nYear = _cc.GetYear(dt);
            int nMonth = _cc.GetMonth(dt);
            int nDay = _cc.GetDayOfMonth(dt);

            int leapMonth = _cc.GetLeapMonth(nYear);

            return (nYear, (leapMonth != 0 && nMonth >= leapMonth) ? nMonth - 1 : nMonth, nDay, nMonth == leapMonth);
        }

        /// <summary>
        /// 把人们口中的农历，转换为公历
        /// </summary>
        public static (int year, int month, int day) FromNongliToGongli(int nYear, int nMonth, int nDay, bool isMonthLeap)
        {
            int leapMonth = _cc.GetLeapMonth(nYear);

            int realNMonth = nMonth;

            if (leapMonth != 0)
            {
                if (nMonth == leapMonth && isMonthLeap || nMonth > leapMonth)
                {
                    realNMonth++;
                }
            }

            DateTime dt = _cc.ToDateTime(nYear, realNMonth, nDay, 0, 0, 0, 0);

            return (dt.Year, dt.Month, dt.Day);
        }
    }

    public static class DateTimeOffsetExtensions
    {
        public static bool IsAM(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.Hour < 12;
        }

        public static DateTimeOffset Add(this DateTimeOffset dateTimeOffset, int hours, int minutes)
        {
            return dateTimeOffset.AddMinutes(hours * 60 + minutes);
        }
    }
}