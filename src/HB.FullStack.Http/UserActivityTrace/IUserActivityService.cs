using System.Threading.Tasks;

namespace HB.FullStack.Server.UserActivityTrace
{
    public interface IUserActivityService
    {
#pragma warning disable CA1054 // URI-like parameters should not be strings
        Task RecordUserActivityAsync(long? signInTokenId, long? userId, string? ip, string? url, string? arguments, int? resultStatusCode);
#pragma warning restore CA1054 // URI-like parameters should not be strings
    }
}