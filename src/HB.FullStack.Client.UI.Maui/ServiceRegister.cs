using HB.FullStack.Client.UI.Maui.File;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui
{
    public static class ServiceRegister
    {
        public static IServiceCollection AddFullStackMaui(this IServiceCollection services, Action<FileManagerOptions> fileManagerOptionConfig)
        {
            //HB.FullStack.Client
            services.AddKVManager();

            //HB.FullStack.Client.UI.Maui
            services.AddFileManager(fileManagerOptionConfig);
            services.AddNetworkManager();

            return services;
        }
    }
}
