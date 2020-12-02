#nullable enable

namespace System
{
    public static class TimeUtil
    {
        public static long UtcNowTicks => DateTimeOffset.UtcNow.Ticks;

        public static long UtcNowUnixTimeSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        public static DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

        public static long UtcNowUnixTimeMilliseconds => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

}