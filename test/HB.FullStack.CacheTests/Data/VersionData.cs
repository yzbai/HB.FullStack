using MessagePack;

namespace HB.FullStack.CacheTests
{
    public class VersionData
    {
        public string Guid { get; set; } = null!;

        public long Timestamp { get; set; }
    }
}