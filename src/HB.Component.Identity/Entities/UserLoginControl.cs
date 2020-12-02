﻿using HB.FullStack.Common.Entities;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.FullStack.Identity.Entities
{
    [KVStoreEntity]
    public class UserLoginControl : Entity
    {
        [Required]
        [KVStoreKey]
        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = null!;


        [EntityProperty]
        public bool LockoutEnabled { get; set; }

        [EntityProperty]
        public DateTimeOffset? LockoutEndDate { get; set; }

        [EntityProperty]
        public long LoginFailedCount { get; set; }

        [EntityProperty]
        public DateTimeOffset? LoginFailedLastTime { get; set; }
    }
}
