using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis.KVStore
{
    public class RedisKVStoreOptions : IOptions<RedisKVStoreOptions>
    {
        public RedisKVStoreOptions Value {
            get {
                return this;
            }
        }


    }
}
