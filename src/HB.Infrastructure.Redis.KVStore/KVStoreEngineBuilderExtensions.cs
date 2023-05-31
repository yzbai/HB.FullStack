using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.KVStore.Config;
using HB.Infrastructure.Redis.KVStore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVStoreEngineBuilderExtensions
    {
        public static IKVStoreEngineBuilder AddRedis(this IKVStoreEngineBuilder builder)
        {
            builder.AddKVStoreEngine<RedisKVStoreEngine>();

            return builder;
        }
    }
}
