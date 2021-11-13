using System;

namespace HB.FullStack.Common
{
    //TODO: 考虑国际
    /// <summary>
    /// 支持公历Date和农历Date的比较
    /// </summary>
    public struct Date : IEquatable<Date>, IComparable<Date>
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

        public override bool Equals(object? obj)
        {
            if (obj is Date date)
            {
                return date == this;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Year, Month, Day, IsNongli, IsMonthLeap);
        }

        public static bool operator ==(Date left, Date right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Date left, Date right)
        {
            return !(left == right);
        }

        public int CompareTo(Date other)
        {
            var gongli1 = IsNongli ? TimeUtil.FromNongliToGongli(Year, Month, Day, IsMonthLeap) : (Year, Month, Day);
            var gongli2 = other.IsNongli ? TimeUtil.FromNongliToGongli(other.Year, other.Month, other.Day, other.IsMonthLeap) : (other.Year, other.Month, other.Day);

            return gongli1.CompareTo(gongli2);
        }

        public static bool operator <(Date left, Date right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Date left, Date right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Date left, Date right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Date left, Date right)
        {
            return left.CompareTo(right) >= 0;
        }

        public bool Equals(Date other)
        {
            if(!other.IsNongli && !IsNongli)
            {
                return Year == other.Year && Month == other.Month && Day == other.Day && IsMonthLeap == other.IsMonthLeap;
            }

            var gongli1 = IsNongli ? TimeUtil.FromNongliToGongli(Year, Month, Day, IsMonthLeap) : (Year, Month, Day);
            var gongli2 = other.IsNongli ? TimeUtil.FromNongliToGongli(other.Year, other.Month,other.Day, other.IsMonthLeap) : (other.Year, other.Month, other.Day);

            return gongli1 == gongli2;
        }
    }
}