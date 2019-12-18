using System.Threading.Tasks;

namespace HB.Framework.Http
{
    /// <summary>
    /// 公开资源，即不需要验证就和获取的资源，需要先获取Token
    /// </summary>
    public interface IPublicResourceTokenManager
    {
        /// <exception cref="EncoderFallbackException"></exception>
        Task<string> GetNewToken(int expiredSeconds = 60);

        Task<bool> CheckToken(string token);
    }
}
