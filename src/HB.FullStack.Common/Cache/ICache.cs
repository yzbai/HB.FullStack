


namespace HB.FullStack.Common.Cache
{
    public interface ICache : IModelCache2, ITimestampCache, ICollectionCache
    {
        void Close();

        void Dispose();

        public static bool IsModelCachable<T>()
        {
            CacheModelDef? modelDef = CacheModelDefFactory.Get<T>();

            return modelDef != null;
        }
    }
}
