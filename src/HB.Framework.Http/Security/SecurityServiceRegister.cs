using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Server.Security;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SecurityServiceRegister
    {
        public static IServiceCollection AddSecurityService(this IServiceCollection services)
        {
            return services.AddSingleton<ISecurityService, DefaultSecurityService>();
        }
    }
}
