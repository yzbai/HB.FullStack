using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services.KVManager
{
    public interface IDbSimpleLocker
    {
        Task<bool> NoWaitLockAsync(string resourceType, string resource, TimeSpan availableTime);

        Task<bool> UnLockAsync(string resourceType, string resource);
    }
}