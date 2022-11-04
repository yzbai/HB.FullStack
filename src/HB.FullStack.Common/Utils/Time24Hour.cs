using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace System
{
    /// <summary>
    /// 不存在24：00
    /// </summary>
    public struct Time24Hour : IEquatable<Time24Hour>, IComparable<Time24Hour>
    {
        private int _day;
        private int _hour;
        private int _minute;

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

        public int Hour
        {
            get { return _hour; }
            set
            {
                if (value >= 0 && value <= 24)
                {
                    if (value == 24)
                    {
                        _day++;
                        _hour = 0;
                    }
                    else
                    {
                        _hour = value;
                    }
                }
                else
                {
                    throw new ArgumentException($"Time24Hour.Hour should not be {value}");
                }
            }
        }

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
        public int HourIn12 => (!IsAm && Hour != 12) ? Hour - 12 : Hour;

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
        /// {day}:{hour}:{minute}
        /// </summary>
        public Time24Hour(string timeString)
        {
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
            if (string.IsNullOrEmpty(timeString) || timeString.Length < 3 || !timeString.Contains(':', Globals.Comparison))
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }
#elif NETSTANDARD2_0
            if (string.IsNullOrEmpty(timeString) || timeString.Length < 3 || !timeString.Contains(":"))
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }
#endif

            string str = timeString.Trim();

            string[] parts = str.Split(':');

            if (parts.Length != 2 && parts.Length != 3)
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }

            int day = 0, hour = 0, minute = 0;

            if (parts.Length == 2)
            {
                hour = Convert.ToInt32(parts[0], Globals.Culture);
                minute = Convert.ToInt32(parts[1], Globals.Culture);
            }
            else if (parts.Length == 3)
            {
                day = Convert.ToInt32(parts[0], Globals.Culture);
                hour = Convert.ToInt32(parts[1], Globals.Culture);
                minute = Convert.ToInt32(parts[2], Globals.Culture);
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

            while (newMinute < 0)
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

        public override bool Equals(object? obj)
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
            if (time1.Day != time2.Day)
            {
                return time1.Day > time2.Day;
            }
            else if (time1.Hour != time2.Hour)
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
            int minutes2 = right.Day * 24 * 60 + right.Hour * 60 + right.Minute;

            return TimeSpan.FromMinutes(minutes1 - minutes2);
        }
    }
}