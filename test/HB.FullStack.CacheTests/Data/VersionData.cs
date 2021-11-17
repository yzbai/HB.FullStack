using MessagePack;

namespace HB.FullStack.CacheTests
{
    [MessagePackObject]
    public class VersionData
    {
        [Key(0)]
        public string Guid { get; set; } = null!;

        [Key(1)]
        public int Version { get; set; }
    }
}