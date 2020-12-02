namespace HB.Infrastructure.Redis.DistributedLock
{
    internal class LoadedLuas
    {
        public byte[] LoadedLockLua { get; internal set; } = null!;
        public byte[] LoadedUnLockLua { get; internal set; } = null!;
        public byte[] LoadedExtendLua { get; internal set; } = null!;
    }
}