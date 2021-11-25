using System;
using System.Globalization;

namespace System
{
    //TODO: 考虑国际
    /// <summary>
    /// 支持公历Date和农历Date的比较
    /// </summary>
    public struct SimpleDate : IEquatable<SimpleDate>, IComparable<SimpleDate>
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int Day { get; set; }

        /// <summary>
        /// 是否是农历
        /// </summary>
        public bool IsNongli { get; set; }

        /// <summary>
        /// Month指的的月是否是闰月
        /// </summary>
        public bool IsMonthLeap { get; set; }

        //public SimpleDate()
        //{
        //    Year = 0;
        //    Month = 0;
        //    Day = 0;
        //    IsMonthLeap = false;
        //    IsNongli = false;
        //}

        public override string ToString()
        {
            return $"{Year}-{Month}-{Day}-{Convert.ToInt32(IsNongli)}-{Convert.ToInt32(IsMonthLeap)}";
        }

        public static SimpleDate ParseExactly(string str)
        {
            if (str.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(str));
            }

            string[] parts = str.Split('-');

            if (parts.Length != 5)
            {
                throw new ArgumentException($"SimpleData无法解析{str}");
            }

            SimpleDate date = new SimpleDate();

            date.Year = Convert.ToInt32(parts[0], CultureInfo.InvariantCulture);
            date.Month = Convert.ToInt32(parts[1], CultureInfo.InvariantCulture);
            date.Day = Convert.ToInt32(parts[2], CultureInfo.InvariantCulture);
            date.IsNongli = Convert.ToBoolean(parts[3], CultureInfo.InvariantCulture);
            date.IsMonthLeap = Convert.ToBoolean(parts[4], CultureInfo.InvariantCulture);

            return date;
        }

        public override bool Equals(object? obj)
        {
            if (obj is SimpleDate date)
            {
                return date == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Month, Day, IsNongli, IsMonthLeap);
        }

        public static bool operator ==(SimpleDate left, SimpleDate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SimpleDate left, SimpleDate right)
        {
            return !(left == right);
        }

        public int CompareTo(SimpleDate other)
        {
            var gongli1 = IsNongli ? TimeUtil.FromNongliToGongli(Year, Month, Day, IsMonthLeap) : (Year, Month, Day);
            var gongli2 = other.IsNongli ? TimeUtil.FromNongliToGongli(other.Year, other.Month, other.Day, other.IsMonthLeap) : (other.Year, other.Month, other.Day);

            return gongli1.CompareTo(gongli2);
        }

        public static bool operator <(SimpleDate left, SimpleDate right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(SimpleDate left, SimpleDate right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(SimpleDate left, SimpleDate right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(SimpleDate left, SimpleDate right)
        {
            return left.CompareTo(right) >= 0;
        }

        public bool Equals(SimpleDate other)
        {
            if (!other.IsNongli && !IsNongli)
            {
                return Year == other.Year && Month == other.Month && Day == other.Day && IsMonthLeap == other.IsMonthLeap;
            }

            var gongli1 = IsNongli ? TimeUtil.FromNongliToGongli(Year, Month, Day, IsMonthLeap) : (Year, Month, Day);
            var gongli2 = other.IsNongli ? TimeUtil.FromNongliToGongli(other.Year, other.Month, other.Day, other.IsMonthLeap) : (other.Year, other.Month, other.Day);

            return gongli1 == gongli2;
        }
    }
}