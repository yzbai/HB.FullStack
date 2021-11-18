﻿namespace HB.Infrastructure.Redis.Cache
{
#pragma warning disable CA1819 // Properties should not return arrays

    public class LoadedLuas
    {
        public byte[] LoadedGetAndRefreshLua { get; set; } = null!;
        public byte[] LoadedEntityGetAndRefreshByDimensionLua { get; set; } = null!;
        public byte[] LoadedEntitySetLua { get; set; } = null!;
        public byte[] LoadedEntityRemoveLua { get; set; } = null!;
        public byte[] LoadedEntityGetAndRefreshLua { get; internal set; } = null!;
        public byte[] LoadedEntityRemoveByDimensionLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesGetAndRefreshLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesGetAndRefreshByDimensionLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesSetLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesRemoveLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesRemoveByDimensionLua { get; internal set; } = null!;

        public byte[] LoadedEntitiesForcedRemoveLua { get; internal set; } = null!;
        public byte[] LoadedEntitiesForcedRemoveByDimensionLua { get; internal set; } = null!;

        public byte[] LoadedSetWithTimestampLua { get; internal set; } = null!;
        public byte[] LoadedRemoveWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedRemoveMultipleWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedCollectionSetWithTimestampLua { get; internal set; } = null!;
        public byte[] LoadedCollectionRemoveItemWithTimestampLua { get; internal set; } = null!;

        public byte[] LoadedCollectionGetAndRefreshWithTimestampLua { get; internal set; } = null!;
    }

#pragma warning restore CA1819 // Properties should not return arrays
}