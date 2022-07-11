using HB.FullStack.Common.Cache.CacheModels;


namespace HB.FullStack.Cache
{
    public interface ICache : IModelCache2, ITimestampCache, ICollectionCache
    {
        void Close();

        void Dispose();

        public static bool IsModelEnabled<TCacheModel>() where TCacheModel : ICacheModel, new()
        {
            CacheModelDef modelDef = CacheModelDefFactory.Get<TCacheModel>();

            return modelDef.IsCacheable;
        }
    }
}
