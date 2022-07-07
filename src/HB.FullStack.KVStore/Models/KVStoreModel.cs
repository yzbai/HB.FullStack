using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.KVStore.KVStoreModels
{
    public abstract class KVStoreModel : Model
    {
        [Required]
        [KVStoreBackupKey]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        public int Version { get; set; } = -1;

        public string LastUser { get; set; } = string.Empty;

        public DateTimeOffset LastTime { get; set; } = TimeUtil.UtcNow;

        //public DateTimeOffset CreateTime { get; /*internal*/ set; } = TimeUtil.UtcNow;

        //public bool Deleted { get; /*internal*/ set; }
    }
}