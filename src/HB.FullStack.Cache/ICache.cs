using HB.FullStack.Cache.CachedCollectionItems;
using HB.FullStack.Cache.CacheItems;

namespace HB.FullStack.Cache
{
    public interface ICache : IModelCache, ICachedItemCache, ICachedCollectionCache
    {
        //void Close();

        //void Dispose();

       

    }
}
