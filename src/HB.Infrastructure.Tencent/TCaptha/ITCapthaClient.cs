using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Infrastructure.Tencent
{
    public interface ITCapthaClient
    {
        Task<bool> VerifyTicket(string appid, string ticket, string randstr, string userIp);
    }
}
