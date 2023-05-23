using HB.FullStack.Common;

namespace System
{
    public static class IExpiredExtensions
    {
        public static bool IsExpired(this IExpired expired, long? nowTimestamp = null, TimeSpan? gap = null)
        {
            ThrowIf.Null(expired, nameof(expired));

            if (expired.ExpiredAt == null)
            {
                return true;
            }

            nowTimestamp ??= TimeUtil.Timestamp;

            if (gap == null)
            {
                return expired.ExpiredAt <= nowTimestamp;
            }
            else
            {
                return expired.ExpiredAt - nowTimestamp <= gap.Value.Ticks;
            }
        }
    }
}