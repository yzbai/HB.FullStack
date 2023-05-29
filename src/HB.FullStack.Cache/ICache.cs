namespace HB.FullStack.Cache
{
    public interface ICache : IModelCache, ITimestampCache, ICollectionCache
    {
        void Close();

        void Dispose();

        CacheModelDef? GetDef<TCacheModel>();

        bool IsModelCachable<T>();

    }
}
