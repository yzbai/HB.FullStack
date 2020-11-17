using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Server.File;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FileServiceRegister
    {
        public static IServiceCollection AddFileService(this IServiceCollection services)
        {
            return services.AddSingleton<IFileService, DefaultFileService>();
        }
    }
}
