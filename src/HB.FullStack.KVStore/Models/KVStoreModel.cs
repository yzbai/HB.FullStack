using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public abstract class KVStoreModel : Model
    {
        [Required]
        [KVStoreBackupKey]
        [CacheModelKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        public long Timestamp { get; set; } = -1;

        public string LastUser { get; set; } = string.Empty;

        public override ModelKind GetKind() => ModelKind.KV;

        //public DateTimeOffset LastTime { get; set; } = TimeUtil.UtcNow;

        //public DateTimeOffset CreateTime { get; /*internal*/ set; } = TimeUtil.UtcNow;

        //public bool Deleted { get; /*internal*/ set; }
    }
}