using HB.FullStack.Database.DbModels;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.KVStoreModels;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Server.Identity.Models
{
    public class LoginControl<TId> : KVStoreModel
    {
        [KVStoreKey]
        public TId UserId { get; set; } = default!;

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEndDate { get; set; }

        public long LoginFailedCount { get; set; }

        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}