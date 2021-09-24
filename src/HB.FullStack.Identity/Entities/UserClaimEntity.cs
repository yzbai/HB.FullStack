﻿
using HB.FullStack.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    internal class UserClaimEntity : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(UserEntity))]
        public Guid UserId { get; set; }

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimType { get; set; } = default!;

        [EntityProperty(MaxLength = 65530, NotNull = true)]
        public string ClaimValue { get; set; } = default!;

        
        public bool AddToJwt { get; set; }
    }
}