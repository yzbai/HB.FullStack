using System.Threading.Tasks;

namespace HB.Infrastructure.Tencent
{
    public interface ITCapthaClient
    {
        /// <exception cref="HB.Infrastructure.Tencent.TCapthaException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="HttpRequestException"></exception>
        Task<bool> VerifyTicketAsync(string appid, string ticket, string randstr, string userIp);
    }
}
