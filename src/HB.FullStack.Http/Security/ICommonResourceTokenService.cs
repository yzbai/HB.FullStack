using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace HB.FullStack.Web
{
    public interface ICommonResourceTokenService
    {
        /// <summary>
        /// 将content变成token，即加密
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        string BuildNewToken(string content);

        bool TryCheckToken(string? protectedToken, [NotNullWhen(true)] out string? content);
    }
}