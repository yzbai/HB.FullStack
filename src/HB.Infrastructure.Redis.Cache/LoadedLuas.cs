namespace HB.Infrastructure.Redis.Cache
{
#pragma warning disable CA1819 // Properties should not return arrays

    public class LoadedLuas
    {
        public byte[] LoadedGetAndRefreshLua { get; set; } = null!;
        public byte[] LoadedModelGetAndRefreshByDimensionLua { get; set; } = null!;
        public byte[] LoadedModelSetLua { get; set; } = null!;
        public byte[] LoadedModelRemoveLua { get; set; } = null!;
        public byte[] LoadedModelGetAndRefreshLua { get; internal set; } = null!;
        public byte[] LoadedModelRemoveByDimensionLua { get; internal set; } = null!;
        public byte[] LoadedModelsGetAndRefreshLua { get; internal set; } = null!;
        public byte[] LoadedModelsGetAndRefreshByDimensionLua { get; internal set; } = null!;
        public byte[] LoadedModelsSetLua { get; internal set; } = null!;
        public byte[] LoadedModelsRemoveLua { get; internal set; } = null!;
        public byte[] LoadedModelsRemoveByDimensionLua { get; internal set; } = null!;

        public byte[] LoadedModelsForcedRemoveLua { get; internal set; } = null!;
        public byte[] LoadedModelsForcedRemoveByDimensionLua { get; internal set; } = null!;

        public byte[] LoadedSetWithTimestampLua { get; internal set; } = null!;
        public byte[] LoadedRemoveWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedRemoveMultipleWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedCollectionSetWithTimestampLua { get; internal set; } = null!;
        public byte[] LoadedCollectionRemoveItemWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedCollectionGetAndRefreshWithTimestampLua { get; internal set; } = null!;
    }

#pragma warning restore CA1819 // Properties should not return arrays
}