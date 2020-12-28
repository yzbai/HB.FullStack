using HB.FullStack.KVStore;
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

        public IList<RedisInstanceSetting> ConnectionSettings { get; set; } = new List<RedisInstanceSetting>();

        public KVStoreSettings KVStoreSettings { get; set; } = new KVStoreSettings();

        public string? ApplicationName { get; set; }

    }
}
