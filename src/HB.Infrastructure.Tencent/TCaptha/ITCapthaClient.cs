using System;
using System.Threading.Tasks;

namespace HB.Infrastructure.Tencent
{
    public interface ITCapthaClient
    {
        
        Task<bool> VerifyTicketAsync(string appid, string ticket, string randstr, string userIp);
    }
}
