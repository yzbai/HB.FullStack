using HB.Framework.KVStore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.KVStore
{
    public class RedisKVStoreOptions : IOptions<RedisKVStoreOptions>
    {
        public RedisKVStoreOptions Value
        {
            get
            {
                return this;
            }
        }

        public IList<RedisInstanceSetting> ConnectionSettings { get; } = new List<RedisInstanceSetting>();

        public KVStoreSettings KVStoreSettings { get; set; } = new KVStoreSettings();

    }
}
