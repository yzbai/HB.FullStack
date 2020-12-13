﻿#nullable enable

namespace System
{
    public struct UtcNowTicks : IEquatable<UtcNowTicks>
    {
        public long Ticks { get; set; }
        public static UtcNowTicks Empty => new UtcNowTicks { Ticks = -1 };

        public bool IsEmpty()
        {
            return Ticks == -1;
        }

        public override bool Equals(object obj)
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
        public static UtcNowTicks UtcNowTicks => new UtcNowTicks { Ticks = DateTimeOffset.UtcNow.Ticks };

        public static long UtcNowUnixTimeSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public static long UtcNowUnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

}