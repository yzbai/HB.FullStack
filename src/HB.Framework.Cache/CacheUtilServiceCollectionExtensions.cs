using HB.Framework.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheUtilServiceCollectionExtensions
    {
        public static IServiceCollection AddCacheUtilities(this IServiceCollection services)
        {
            services.AddSingleton<IFrequencyChecker, FrequencyChecker>();

            return services;
        }
    }
}
