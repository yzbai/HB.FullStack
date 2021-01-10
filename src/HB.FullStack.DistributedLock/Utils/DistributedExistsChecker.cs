#nullable enable

using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
namespace HB.FullStack.Lock
{
    //public class DistributedExistsChecker
    //{
    //    private readonly string _applicationName;

    //    private readonly IDistributedCache _cache;

    //    public DistributedExistsChecker(IDistributedCache cache, string applicaionName)
    //    {
    //        _cache = cache;
    //        _applicationName = applicaionName;
    //    }

    //    public async Task<bool> CheckAsync(string resourceType, string resource, TimeSpan aliveTimeSpan)
    //    {
    //        string key = GetKey(resourceType, resource);

    //        string? value = await _cache.GetStringAsync(key).ConfigureAwait(false);

    //        if (string.IsNullOrEmpty(value))
    //        {
    //            await _cache.SetStringAsync(key, "Hit", new DistributedCacheEntryOptions() { AbsoluteExpirationRelativeToNow = aliveTimeSpan }).ConfigureAwait(false);
    //            return true;
    //        }

    //        return false;
    //    }

    //    public Task RemoveAsync(string resourceType, string resource)
    //    {
    //        string key = GetKey(resourceType, resource);

    //        return _cache.RemoveAsync(key);
    //    }

    //    private string GetKey(string resourceType, string resource)
    //    {
    //        return $"{_applicationName}{resourceType}{resource}";
    //    }
    //}
}