
using HB.FullStack.Database.Def;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Identity.Entities
{
    public class UserLoginControl : KVStoreEntity
    {
        [Required]
        [KVStoreKey]
        public long UserId { get; set; }


        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEndDate { get; set; }

        public long LoginFailedCount { get; set; }

        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}
