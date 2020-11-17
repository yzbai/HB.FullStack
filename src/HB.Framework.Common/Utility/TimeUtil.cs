#nullable enable

namespace System
{
    public static class TimeUtil
    {
        //TODO: 对系统中的DateTime, DateTimeOffset,DateTime.UtcNow做出梳理和清理

        public static long CurrentTimestampSeconds()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
    }
}