using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Services.DbLocker
{
    public interface IDbSimpleLocker
    {
        Task<bool> NoWaitLockAsync(string resourceType, string resource, TimeSpan availableTime);

        Task<bool> UnLockAsync(string resourceType, string resource);
    }
}