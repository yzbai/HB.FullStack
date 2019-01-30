using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class TimeUtil
    {
        private const string YearMonthDayFormat = "yyyy-MM-dd";
        private const string YearMonthDayHourMinuteSecondFormat = "yyyy-MM-dd HH:mm:ss";

        public static string GetYearMonthDay(DateTime dateTime)
        {
            return dateTime.ToString(YearMonthDayFormat, GlobalSettings.Culture);
        }

        public static string GetTodayYearMonthDay()
        {
            return GetYearMonthDay(DateTime.Now);
        }

        //TODO: 对系统中的DateTime, DateTimeOffset,DateTime.UtcNow做出梳理和清理

        public static long CurrentTimestampSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public static long ToTimestampSeconds(DateTimeOffset dt)
        {
            return dt.ToUnixTimeSeconds();
        }

        public static DateTimeOffset ToDateTimeOffset(long timestampSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);
        }

        /// <summary>
        /// 与今天相比
        /// </summary>
        /// <param name="yearMonthDay">yyyy-MM-dd</param>
        /// <returns>
        /// 0:equal
        /// -1: input is smaller
        /// 1: input is bigger
        /// </returns>
        public static int CompareToToday(string yearMonthDay)
        {
            //TODO: add parameter Check
            string today = GetTodayYearMonthDay();
            string[] todayResult = today.Split(new char[] { ',', '-' });
            string[] inputResult = yearMonthDay.Split(new char[] { ',', '-' });

            int result = Convert.ToInt32(inputResult[0], GlobalSettings.Culture)
                .CompareTo(Convert.ToInt32(todayResult[0], GlobalSettings.Culture));

            if (result != 0)
            {
                return result;
            }

            result = Convert.ToInt32(inputResult[1], GlobalSettings.Culture)
                .CompareTo(Convert.ToInt32(todayResult[1], GlobalSettings.Culture));

            if (result != 0)
            {
                return result;
            }

            return Convert.ToInt32(inputResult[2], GlobalSettings.Culture)
                .CompareTo(Convert.ToInt32(todayResult[2], GlobalSettings.Culture));
        }

        public static string GetDateTimeString(DateTime dateTime)
        {
            return dateTime.ToString(YearMonthDayHourMinuteSecondFormat, GlobalSettings.Culture);
        }
        public static string GetNowString()
        {
            return GetDateTimeString(DateTime.Now);
        }
    }
}
