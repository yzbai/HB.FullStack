using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Caching.Distributed
{
    public static class DistributedCacheEntryOptionsExtension
    {
        public static void Check(this DistributedCacheEntryOptions options)
        {
            if (options == null)
            {
                return;
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                options.AbsoluteExpiration = now + options.AbsoluteExpirationRelativeToNow;
            }
        }
    }
}