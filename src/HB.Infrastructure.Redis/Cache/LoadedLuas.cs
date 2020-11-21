namespace HB.Infrastructure.Redis.Cache
{
    /// <summary>
    /// Entity in Redis:
    /// 
    ///                                 |------ abexp    :  value
    /// Guid----------------------------|------ slidexp  :  value        //same as IDistributed way
    ///                                 |------ data     :  jsonString
    /// 
    /// 
    ///                                 |------- DimensionKeyValue_1   :  Guid
    /// EntityName_DimensionKeyName-----|......
    ///                                 |------- DimensionKeyValue_n   :  Guid
    ///                                 
    /// 所以EntityName_DimensionKeyName 这个key是一个索引key
    /// </summary>

    internal class LoadedLuas
    {
        public byte[] LoadedSetLua { get; set; } = null!;

        public byte[] LoadedGetAndRefreshLua { get; set; } = null!;
        public byte[] LoadedEntityGetAndRefreshByDimensionLua { get; set; } = null!;
        public byte[] LoadedEntitySetLua { get; set; } = null!;
        public byte[] LoadedEntityRemoveLua { get; set; } = null!;
        public byte[] LoadedEntityGetAndRefreshLua { get; internal set; } = null!;
        public byte[] LoadedEntityRemoveByDimensionLua { get; internal set; } = null!;
    }
}
