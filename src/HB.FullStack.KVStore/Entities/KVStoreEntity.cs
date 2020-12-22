using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Entities;

namespace HB.FullStack.KVStore.Entities
{
    public abstract class KVStoreEntity : Entity
    {
        [Required]
        [KVStoreBackupKey]
        [CacheKey]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();

    }
}
