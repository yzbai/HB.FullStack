using System;

namespace HB.FullStack.Cache
{
    public interface ICacheModelDefFactory
    {
        CacheModelDef? GetDef<T>();

        CacheModelDef? GetDef(Type type);
    }
}