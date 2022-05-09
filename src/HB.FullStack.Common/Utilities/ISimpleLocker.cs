using System;

namespace System
{
    public interface ISimpleLocker
    {
        bool NoWaitLock(string resourceType, string resource, TimeSpan expiryTime);
        bool UnLock(string resourceType, string resource);
    }
}