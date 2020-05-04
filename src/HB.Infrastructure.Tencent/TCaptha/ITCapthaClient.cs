using System.Threading.Tasks;

namespace HB.Infrastructure.Tencent
{
    public interface ITCapthaClient
    {
        Task<bool> VerifyTicket(string appid, string ticket, string randstr, string userIp);
    }
}
