namespace HB.Infrastructure.Redis.KVStore
{
    internal class LoadedLuas
    {
        public byte[] LoadedBatchAddLua { get; internal set; } = null!;
        public byte[] LoadedBatchUpdateLua { get; internal set; } = null!;
        public byte[] LoadedBatchDeleteLua { get; internal set; } = null!;
        public byte[] LodedeBatchAddOrUpdateLua { get; internal set; } = null!;
        public byte[] LoadedBatchGetLua { get; internal set; } = null!;
        public byte[] LoadedGetAllLua { get; internal set; } = null!;
    }
}
