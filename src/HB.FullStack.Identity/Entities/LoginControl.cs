using HB.FullStack.Database.Entities;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Identity.Entities
{
    public class LoginControl : KVStoreEntity
    {
        [NoEmptyGuid]
        [KVStoreKey]
        [MessagePack.Key(7)]
        public Guid UserId { get; set; }

        [MessagePack.Key(8)]
        public bool LockoutEnabled { get; set; }

        [MessagePack.Key(9)]
        public DateTimeOffset? LockoutEndDate { get; set; }

        [MessagePack.Key(10)]
        public long LoginFailedCount { get; set; }

        [MessagePack.Key(11)]
        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}