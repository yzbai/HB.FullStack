
using HB.FullStack.Common;
using HB.FullStack.Database.Entities;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.ModelObjects
{
    public class UserClaim : GuidModelObject
    {
        [NoEmptyGuid]
        public Guid UserId { get; set; }

        public string ClaimType { get; set; } = default!;

        public string ClaimValue { get; set; } = default!;
        
        public bool AddToJwt { get; set; }
    }
}
