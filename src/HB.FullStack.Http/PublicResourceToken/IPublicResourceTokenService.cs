using System;
using System.Threading.Tasks;

namespace HB.FullStack.Server
{
    /// <summary>
    /// 公开资源，即不需要验证就和获取的资源，需要先获取Token
    /// </summary>
    public interface IPublicResourceTokenService
    {
        /// <exception cref="CacheException"></exception>
        Task<string> GetNewTokenAsync(int expiredSeconds = 60);

        /// <exception cref="CacheException"></exception>
        Task<bool> CheckTokenAsync(string? token);
    }
}
