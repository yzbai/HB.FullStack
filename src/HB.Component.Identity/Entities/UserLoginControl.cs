using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HB.Component.Identity.Entities
{
    [KVStoreEntity]
    public class UserLoginControl : Entity
    {
        [Required]
        [KVStoreKey]
        [ForeignKey(typeof(IdentityUser))]
        [GuidEntityProperty(NotNull = true)]
        public string UserGuid { get; set; } = null!;


        [EntityProperty]
        public bool LockoutEnabled { get; set; }

        [EntityProperty]
        public DateTimeOffset? LockoutEndDate { get; set; }

        [EntityProperty]
        public long LoginFailedCount { get; set; }

    }
}
