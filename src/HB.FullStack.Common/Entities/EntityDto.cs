using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Entities
{
    public class EntityDto : ValidatableObject
    {
        [Required]
        public string Guid { get; set; } = SecurityUtil.CreateUniqueToken();
    }
}