using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common
{
    /// <summary>
    /// 不存在24：00
    /// </summary>
    public struct Time24Hour : IEquatable<Time24Hour>
    {
        private int _day;

        /// <summary>
        /// 为了应对跨天的时间块
        /// Day = 0 为当前天
        /// Day = 1 为后一天
        /// </summary>
        public int Day
        {
            get => _day;
            set => _day = value;
        }

        private int _hour;

        public int Hour
        {
            get { return _hour; }
            set
            {
                if (value >= 0 && value < 24)
                {
                    _hour = value;
                }
                else
                {
                    throw new ArgumentException($"Time24Hour.Hour should not be {value}");
                }
            }
        }

        [JsonIgnore]
        public int HourIn12 => (!IsAm && Hour != 12) ? Hour - 12 : Hour;

        private int _minute;

        public int Minute
        {
            get { return _minute; }
            set
            {
                if (value >= 0 && value < 60)
                {
                    _minute = value;
                }
                else
                {
                    throw new ArgumentException($"Time24Hour.Minute should not be {value}");
                }
            }
        }

        [JsonIgnore]
        public bool IsAm { get => Hour < 12; }
        public static Time24Hour LocalNow
        {
            get
            {
                DateTime dt = TimeUtil.LocalNow;

                return new Time24Hour(dt.Hour, dt.Minute);
            }
        }

        public Time24Hour(int hour24, int minute, int day = 0)
        {
            _hour = 0;
            _minute = 0;
            _day = 0;

            Day = day;
            Hour = hour24;
            Minute = minute;
        }

        /// <summary>
        /// example:
        /// 上午9：12
        /// pm12:02
        /// 0:13:34
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public Time24Hour(string timeString)
        {
#if NETSTANDARD2_0
            if (string.IsNullOrEmpty(timeString) || timeString.Length < 3 || !timeString.Contains(":"))
#endif
#if NETSTANDARD2_1
            if (string.IsNullOrEmpty(timeString) || timeString.Length < 3 || !timeString.Contains(':', GlobalSettings.Comparison))
#endif
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }

            bool? isAM = null;
            string str = timeString.Trim();

            if (!char.IsNumber(str, 0))
            {
                string ampmStr = str.Substring(0, 2);

                isAM = ampmStr.IsIn("上午", "AM", "am", "Am", "aM");

                str = str.Remove(0, 2);
            }

            string[] parts = str.Split(':');

            if (parts.Length != 2 && parts.Length != 3)
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }

            int day = 0, hour = 0, minute = 0;

            if(parts.Length == 2)
            {
                hour = Convert.ToInt32(parts[0], GlobalSettings.Culture);
                minute = Convert.ToInt32(parts[1], GlobalSettings.Culture);
            }
            else if(parts.Length == 3)
            {
                day = Convert.ToInt32(parts[0], GlobalSettings.Culture);
                hour = Convert.ToInt32(parts[1], GlobalSettings.Culture);
                minute = Convert.ToInt32(parts[2], GlobalSettings.Culture);
            }

            if (isAM.HasValue)
            {
                if (!isAM.Value && hour != 12)
                {
                    hour += 12;
                }
            }

            _hour = 0;
            _minute = 0;
            _day = 0;

            Day = day;
            Hour = hour;
            Minute = minute;

            if (Hour < 0 || Hour >= 24 || Minute < 0 || Minute >= 60)
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }
        }

        public Time24Hour AddTime(int changedHour, int changedMinute)
        {
            int newDay = Day;
            int newhour = Hour + changedHour;
            int newMinute = Minute + changedMinute;

            while(newMinute < 0)
            {
                newhour--;
                newMinute += 60;
            }

            while (newMinute >= 60)
            {
                newhour++;
                newMinute -= 60;
            }

            while (newhour < 0)
            {
                newDay--;
                newhour += 24;
            }

            while (newhour >= 24)
            {
                newDay++;
                newhour -= 24;
            }

            return new Time24Hour(newhour, newMinute, newDay);
        }

        public override bool Equals(object obj)
        {
            if (obj is Time24Hour time)
            {
                return this == time;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_hour, _minute);
        }

        public bool Equals(Time24Hour other)
        {
            return this == other;
        }

        public int CompareTo(Time24Hour other)
        {
            if (this > other)
            {
                return 1;
            }
            if (this < other)
            {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return $"{Day}:{Hour}:{Minute}";
        }

        public static bool operator ==(Time24Hour time1, Time24Hour time2)
        {
            return time1.Hour == time2.Hour && time1.Minute == time2.Minute;
        }

        public static bool operator !=(Time24Hour time1, Time24Hour time2)
        {
            return !(time1 == time2);
        }

        public static bool operator >(Time24Hour time1, Time24Hour time2)
        {
            if (time1.Hour != time2.Hour)
            {
                return time1.Hour > time2.Hour;
            }
            else
            {
                return time1.Minute > time2.Minute;
            }
        }

        public static bool operator <(Time24Hour time1, Time24Hour time2)
        {
            return time1 != time2 && !(time1 > time2);
        }

        public static bool operator >=(Time24Hour time1, Time24Hour time2)
        {
            return time1 == time2 || time1 > time2;
        }

        public static bool operator <=(Time24Hour time1, Time24Hour time2)
        {
            return time1 == time2 || time1 < time2;
        }

        public static TimeSpan operator -(Time24Hour item1, Time24Hour item2)
        {
            return Subtract(item1, item2);
        }

        public static TimeSpan Subtract(Time24Hour left, Time24Hour right)
        {
            int minutes1 = left.Day * 24 * 60 + left.Hour * 60 + left.Minute;
            int minutes2 = left.Day * 24 * 60 + right.Hour * 60 + right.Minute;

            return TimeSpan.FromMinutes(minutes1 - minutes2);
        }

    }

    //public static class Time24HourExtensions
    //{
    //    public static Time24HourInDouble ToDoubleFormat(this Time24Hour time24Hour)
    //    {
    //        return new Time24HourInDouble { Hour = time24Hour.Hour, Minute = time24Hour.Minute };
    //    }

    //}

    //public struct Time24HourInDouble : IEquatable<Time24HourInDouble>
    //{
    //    private int _hour;
    //    public int Hour
    //    {
    //        get { return _hour; }
    //        set
    //        {
    //            if (value >= 0 && value < 24)
    //            {
    //                _hour = value;
    //            }
    //            else
    //            {
    //                throw new ArgumentException($"Time24Hour.Hour should not be {value}");
    //            }
    //        }
    //    }

    //    private double _minute;

    //    public double Minute
    //    {
    //        get { return _minute; }
    //        set
    //        {
    //            if (value >= 0 && value < 60)
    //            {
    //                _minute = value;
    //            }
    //            else
    //            {
    //                throw new ArgumentException($"Time24Hour.Minute should not be {value}");
    //            }
    //        }
    //    }

    //    public Time24HourInDouble(int hour, double minute)
    //    {
    //        _hour = 0;
    //        _minute = 0;
    //        Hour = hour;
    //        Minute = minute;
    //    }

    //    public Time24HourInDouble AddMinute(double minute)
    //    {
    //        double curMinute = Minute;
    //        int curHour = Hour;

    //        curMinute += minute;

    //        if (curMinute >= 60)
    //        {
    //            curMinute -= 60;

    //            if (curHour == 23)
    //            {
    //                curHour = 0;
    //            }
    //            else
    //            {
    //                curHour++;
    //            }
    //        }

    //        return new Time24HourInDouble(curHour, curMinute);
    //    }

    //    public Time24HourInDouble SubMinute(double minute)
    //    {
    //        double curMinute = Minute;
    //        int curHour = Hour;

    //        if (curMinute >= minute)
    //        {
    //            curMinute -= minute;
    //        }
    //        else
    //        {
    //            curMinute = curMinute + 60 - minute;

    //            if (curHour == 0)
    //            {
    //                curHour = 23;
    //            }
    //            else
    //            {
    //                curHour--;
    //            }
    //        }

    //        return new Time24HourInDouble(curHour, curMinute);
    //    }

    //    public static bool operator ==(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        return time1.Hour == time2.Hour && time1.Minute == time2.Minute;
    //    }

    //    public static bool operator !=(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        return !(time1 == time2);
    //    }

    //    public static bool operator >(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        if (time1.Hour != time2.Hour)
    //        {
    //            return time1.Hour > time2.Hour;
    //        }
    //        else
    //        {
    //            return time1.Minute > time2.Minute;
    //        }
    //    }

    //    public static bool operator <(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        return time1 != time2 && !(time1 > time2);
    //    }

    //    public static bool operator >=(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        return time1 == time2 || time1 > time2;
    //    }

    //    public static bool operator <=(Time24HourInDouble time1, Time24HourInDouble time2)
    //    {
    //        return time1 == time2 || time1 < time2;
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        if (obj is Time24HourInDouble time)
    //        {
    //            return this == time;
    //        }

    //        return false;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return HashCode.Combine(_hour, _minute);
    //    }

    //    public bool Equals(Time24HourInDouble other)
    //    {
    //        return this == other;
    //    }

    //    public int CompareTo(Time24HourInDouble other)
    //    {
    //        if (this > other)
    //        {
    //            return 1;
    //        }
    //        if (this < other)
    //        {
    //            return -1;
    //        }
    //        return 0;
    //    }
    //}
}