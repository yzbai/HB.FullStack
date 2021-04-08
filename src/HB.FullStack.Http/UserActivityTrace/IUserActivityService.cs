using System;
using System.Threading.Tasks;

using StackExchange.Redis;

namespace HB.FullStack.WebApi.UserActivityTrace
{
    public interface IUserActivityService
    {

#pragma warning disable CA1054 // URI-like parameters should not be strings
        Task RecordUserActivityAsync(long? signInTokenId, long? userId, string? ip, string? url, string? httpMethod, string? arguments, int? resultStatusCode, string? resultType, ErrorCode? errorCode);
#pragma warning restore CA1054 // URI-like parameters should not be strings
    }
}