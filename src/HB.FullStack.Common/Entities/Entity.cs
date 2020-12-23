#nullable enable

using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("HB.FullStack.Database")]
[assembly: InternalsVisibleTo("HB.FullStack.Database.ClientExtension")]
[assembly: InternalsVisibleTo("HB.FullStack.Cache")]
[assembly: InternalsVisibleTo("HB.FullStack.KVStore")]

namespace HB.FullStack.Common.Entities
{
    public abstract class Entity : ValidatableObject
    {


        public int Version { get; internal set; } = -1;

        public string LastUser { get; internal set; } = string.Empty;

        /// <summary>
        /// UTC 时间
        /// </summary>
        public DateTimeOffset LastTime { get; internal set; } = TimeUtil.UtcNow;

        public bool Deleted { get; internal set; }
    }
}