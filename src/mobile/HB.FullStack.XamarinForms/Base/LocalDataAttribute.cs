using System;

namespace System
{
    [AttributeUsage(AttributeTargets.Class)]
    public class LocalDataAttribute : Attribute
    {
        public TimeSpan ExpiryTime { get; set; }

        public bool AllowOfflineWrite { get; set; }

        public bool AllowOfflineRead { get; set; }
        
        public bool NeedLogined { get; set; }

        public LocalDataAttribute(int expiryMinutes, bool needLogined, bool allowOfflineRead, bool allowOfflineWrite)
        {
            ExpiryTime = TimeSpan.FromMinutes(expiryMinutes);

            NeedLogined = needLogined;
            AllowOfflineRead = allowOfflineRead;
            AllowOfflineWrite = allowOfflineWrite;
        }
    }
}