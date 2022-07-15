using HB.FullStack.Database.DBModels;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.KVStoreModels;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Identity.Models
{
    public class LoginControl : KVStoreModel
    {
        [NoEmptyGuid]
        [KVStoreKey]
        public Guid UserId { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEndDate { get; set; }

        public long LoginFailedCount { get; set; }

        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}