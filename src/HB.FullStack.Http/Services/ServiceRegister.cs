using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Server.Services;
using HB.FullStack.Server.WebLib.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StsTokenServiceServiceRegister
    {
        public static IServiceCollection AddDirectoryTokenService(this IServiceCollection services, Action<DirectoryOptions> configDirectoryOptions)
        {
            services.Configure(configDirectoryOptions);

            services.AddSingleton<IDirectoryTokenService, DirectoryTokenService>();

            return services;
        }
    }
}
