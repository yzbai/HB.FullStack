﻿
using HB.FullStack.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    public class UserClaim : FlackIdEntity
    {
        [LongId]
        [ForeignKey(typeof(User))]
        public long UserId { get; set; }

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        
        public bool AddToJwt { get; set; }
    }
}
