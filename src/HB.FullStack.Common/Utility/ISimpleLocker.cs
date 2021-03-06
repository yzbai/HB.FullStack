using System;

namespace HB.FullStack.Common
{
    public interface ISimpleLocker
    {
        bool NoWaitLock(string resourceType, string resource, TimeSpan expiryTime);
        bool UnLock(string resourceType, string resource);
    }
}