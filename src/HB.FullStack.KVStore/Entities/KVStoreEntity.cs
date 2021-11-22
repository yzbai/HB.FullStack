using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.KVStore.Entities
{
    public abstract class KVStoreEntity : Entity
    {
        [Required]
        [KVStoreBackupKey]
        [CacheKey]
        [MessagePack.Key(5)]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

        [MessagePack.Key(6)]
        public override string LastUser { get; set; } = string.Empty;
    }
}