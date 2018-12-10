using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedQueueServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedQueue(this IServiceCollection services)
        {
            return services;
        }
    }
}
