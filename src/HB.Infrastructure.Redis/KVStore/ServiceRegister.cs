﻿using HB.Infrastructure.Redis.KVStore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class KVStoreServiceRegister
    {
        public static IServiceCollection AddRedisKVStore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<RedisKVStoreOptions>(configuration);


        }

        public static IServiceCollection AddRedisKVStore(this IServiceCollection services, Action<RedisKVStoreOptions> action)
        {
            services.AddOptions();
            services.Configure(action);
        }
    }
}
