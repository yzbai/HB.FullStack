using System.ComponentModel.DataAnnotations;

namespace HB.Framework.Common.Entities
{
    public class EntityDto : ValidatableObject
    {
        [Required]
        public string Guid { get; set; } = null!;
    }
}