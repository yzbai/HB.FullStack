﻿using HB.FullStack.Database.Entities;
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
        public Guid UserId { get; set; }

        public bool LockoutEnabled { get; set; }

        public DateTimeOffset? LockoutEndDate { get; set; }

        public long LoginFailedCount { get; set; }

        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}