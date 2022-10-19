namespace HB.FullStack.Cache
{
    public interface ICache : IModelCache2, ITimestampCache, ICollectionCache
    {
        void Close();

        void Dispose();

        CacheModelDef? GetDef<TCacheModel>();

        bool IsModelCachable<T>();

    }
}
