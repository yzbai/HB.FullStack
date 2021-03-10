using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Cache
{
    public enum CacheErrorCode
    {
        CacheSlidingTimeBiggerThanMaxAlive,
        CacheEntityNotHaveKeyAttribute,
        ConvertError,
        CacheLoadedLuaNotFound,
        CacheInstanceNotFound,
        NoSuchDimensionKey,
        NotEnabledForEntity,
        Unkown,
        NotACacheEntity,
        UnkownButDeleted
    }
}
