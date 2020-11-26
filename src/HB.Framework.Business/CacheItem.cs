using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Business
{
    public abstract class CacheItem<T> where T : class
    {
        public CacheItem(string key)
        {
            Key = key;
        }

        public abstract TimeSpan? AbsoluteExpirationRelativeToNow { get; }
        public abstract TimeSpan? SlidingExpiration { get; }

        public string Key { get; internal set; }

        public T? Value { get; internal set; }
    }
}

