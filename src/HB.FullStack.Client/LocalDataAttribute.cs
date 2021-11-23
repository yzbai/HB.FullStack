using System;

namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LocalDataAttribute : Attribute
    {
        public int ExpirySeconds { get; }

        public bool AllowOfflineWrite { get; }

        public bool AllowOfflineRead { get; }

        public bool NeedLogined { get; }

        public LocalDataAttribute(int expirySeconds, bool needLogined, bool allowOfflineRead, bool allowOfflineWrite)
        {
            ExpirySeconds = expirySeconds;

            NeedLogined = needLogined;
            AllowOfflineRead = allowOfflineRead;
            AllowOfflineWrite = allowOfflineWrite;
        }
    }
}