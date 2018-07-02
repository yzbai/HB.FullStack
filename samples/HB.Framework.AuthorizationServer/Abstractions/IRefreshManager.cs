using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.AuthorizationServer.Abstractions
{
    public interface IRefreshManager
    {
        Task<RefreshResult> RefreshAccessTokenAsync(RefreshContext context);
    }
}
