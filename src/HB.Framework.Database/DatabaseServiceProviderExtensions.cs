using HB.Framework.Database;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DatabaseServiceProviderExtensions
    {
        public static IDatabase GetDatabase(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<IDatabase>();
        }
    }
}
