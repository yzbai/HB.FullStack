using System;
using System.Globalization;

namespace HB.FullStack.Mobile.Controls.Clock
{
    /// <summary>
    /// 不存在24：00
    /// </summary>
    public struct Time24Hour : IEquatable<Time24Hour>
    {
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

        public Time24Hour(int hour, int minute)
        {
            _hour = 0;
            _minute = 0;
            Hour = hour;
            Minute = minute;
        }

        /// <summary>
        /// example:
        /// 上午9：12
        /// pm12:02
        /// </summary>
        /// <param name="timeString"></param>
        /// <returns></returns>
        public Time24Hour(string timeString)
        {
            if (string.IsNullOrEmpty(timeString) || timeString.Length < 3 || !timeString.Contains(':', GlobalSettings.Comparison))
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }

            bool isAM = true;
            string str = timeString.Trim();

            if (!char.IsNumber(str, 0))
            {
                string ampmStr = str.Substring(0, 2);

                isAM = ampmStr.IsIn("上午", "AM", "am");

                str = str.Remove(0, 2);
            }

            string[] parts = str.Split(':');

            if (parts.Length != 2)
            {
                throw new ArgumentException("Time24Hour初始化时间字符串格式不对", nameof(timeString));
            }

            int hour = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
            int minute = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);

            if (!isAM && hour != 12)
            {
                hour += 12;
            }

            _hour = 0;
            _minute = 0;
            Hour = hour;
            Minute = minute;
        }

        public Time24Hour AddMinute(int minute)
        {
            if (minute < 0)
            {
                throw new ArgumentException($"Time24Hour.AddMinute not accept negtive.{minute}");
            }

            minute += Minute;
            int addedHour = minute / 60;
            int curMinute = minute % 60;

            int curHour = Hour + addedHour;

            curHour %= 24;

            return new Time24Hour(curHour, curMinute);
        }

        public static bool operator ==(Time24Hour time1, Time24Hour time2)
        {
            return time1.Hour == time2.Hour && time1.Minute == time2.Minute;
        }

        public static bool operator !=(Time24Hour time1, Time24Hour time2)
        {
            return !(time1 == time2);
        }

        internal Time24Hour MinusTime(int changedHour, int changedMinute)
        {
            if (changedHour < 0 || changedHour < 0)
            {
                throw new ArgumentException($"Time24Hour.AddTime not accept negtive.{changedHour}:{changedMinute}");
            }

            changedHour += (changedMinute / 60);

            int curMinute;

            if (Minute >= changedMinute)
            {
                curMinute = Minute - changedMinute;
            }
            else
            {
                changedHour++;
                curMinute = 60 + Minute - changedMinute;
            }

            int curHour;

            changedHour %= 24;

            if (Hour >= changedHour)
            {
                curHour = Hour - changedHour;
            }
            else
            {
                curHour = 24 + Hour - changedHour;
            }

            return new Time24Hour(curHour, curMinute);
        }

        public Time24Hour AddTime(int addedHour, int addedMinute)
        {
            if (addedHour < 0 || addedMinute < 0)
            {
                throw new ArgumentException($"Time24Hour.AddTime not accept negtive.{addedHour}:{addedMinute}");
            }

            int curMinute = (addedMinute + Minute) % 60;
            int curHour = Hour + addedHour + (addedMinute + Minute) / 60;

            curHour %= 24;

            return new Time24Hour(curHour, curMinute);
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
            return $"{Hour}:{Minute}";
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