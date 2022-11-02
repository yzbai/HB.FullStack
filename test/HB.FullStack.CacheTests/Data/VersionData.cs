using System;

using HB.FullStack.Database.DbModels;

using MessagePack;

namespace HB.FullStack.CacheTests
{

    public class VersionData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long Timestamp { get; set; } = TimeUtil.Timestamp;
    }
}