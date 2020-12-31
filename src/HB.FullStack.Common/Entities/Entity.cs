#nullable enable

using System;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]
[assembly: InternalsVisibleTo("HB.FullStack.Repository")]

namespace HB.FullStack.Common.Entities
{
    public abstract class Entity : ValidatableObject
    {


        public int Version { get; set; } = -1;

        public string LastUser { get; internal set; } = string.Empty;

        /// <summary>
        /// UTC 时间
        /// </summary>
        public DateTimeOffset LastTime { get; internal set; } = TimeUtil.UtcNow;

        public DateTimeOffset CreateTime { get; internal set; } = TimeUtil.UtcNow;

        public bool Deleted { get; internal set; }
    }
}