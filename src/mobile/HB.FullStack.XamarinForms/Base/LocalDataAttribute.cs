using System;

namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LocalDataAttribute : Attribute
    {
        public int ExpiryMinutes { get; }

        public bool AllowOfflineWrite { get; }

        public bool AllowOfflineRead { get; }

        public bool NeedLogined { get; }

        public LocalDataAttribute(int expiryMinutes, bool needLogined, bool allowOfflineRead, bool allowOfflineWrite)
        {
            ExpiryMinutes = expiryMinutes;

            NeedLogined = needLogined;
            AllowOfflineRead = allowOfflineRead;
            AllowOfflineWrite = allowOfflineWrite;
        }
    }
}