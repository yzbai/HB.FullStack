#nullable enable

namespace System
{
    public struct UtcNowTicks : IEquatable<UtcNowTicks>
    {
        public static UtcNowTicks Empty => new UtcNowTicks(-1);
        public static UtcNowTicks Instance => new UtcNowTicks(DateTimeOffset.UtcNow.Ticks);

        private UtcNowTicks(long ticks)
        {
            Ticks = ticks;
        }

        public long Ticks { get; internal set; }

        public bool IsEmpty()
        {
            return Ticks == -1;
        }

        public override bool Equals(object? obj)
        {
            if (obj is UtcNowTicks unt)
            {
                return unt.Ticks == Ticks;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Ticks.GetHashCode();
        }

        public static bool operator ==(UtcNowTicks left, UtcNowTicks right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UtcNowTicks left, UtcNowTicks right)
        {
            return !(left == right);
        }

        public bool Equals(UtcNowTicks other)
        {
            return other.Ticks == Ticks;
        }

        //public static implicit operator long(UtcNowTicks unt)
        //{
        //    return unt.Ticks;
        //}

        //public static implicit operator UtcNowTicks(long ticks)
        //{
        //    return new UtcNowTicks { Ticks = ticks };
        //}

        //public long ToInt64()
        //{
        //    return Ticks;
        //}

        //public UtcNowTicks ToUtcNowTicks()
        //{
        //    return this;
        //}
    }

    public static class TimeUtil
    {
        public static UtcNowTicks UtcNowTicks => UtcNowTicks.Instance;

        public static long UtcNowUnixTimeSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public static long UtcNowUnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public static DateTime LocalNow => DateTime.Now;

        public static DateTimeOffset CreateOnlyTime(int day, int hour, int minutes)
        {
            //[0001/1/1 0:00:00 +00:00]
            return new DateTimeOffset(1, 1, day, hour, minutes, 0, TimeSpan.Zero);
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