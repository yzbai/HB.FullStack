#nullable enable


using System;

namespace HB.FullStack.Lock.Memory
{
    public class MemoryLockResourceExpiryInfo
    {
        public long Timestamp { get; set; }

        public long ExpiryMilliseconds { get; set; }

        public string ResourceValue { get; set; } = null!;

        public MemoryLockResourceExpiryInfo(long timestamp, long expiryMilliseconds, string resourceValue)
        {
            Timestamp = timestamp;
            ExpiryMilliseconds = expiryMilliseconds;
            ResourceValue = resourceValue;
        }

    }
}