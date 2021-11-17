#nullable enable

using MessagePack;

using System;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]

namespace HB.FullStack.Common
{
    [MessagePackObject]
    public abstract class Entity : ValidatableObject
    {
        [Key(0)]
        public int Version { get; set; } = -1;

        [Key(1)]
        public abstract string LastUser { get; set; }

        /// <summary>
        /// UTC 时间
        /// </summary>
        [Key(2)]
        public DateTimeOffset LastTime { get; set; } = TimeUtil.UtcNow;

        [Key(3)]
        public DateTimeOffset CreateTime { get; /*internal*/ set; } = TimeUtil.UtcNow;

        [Key(4)]
        public bool Deleted { get; /*internal*/ set; }
    }
}