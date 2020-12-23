using System;
using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// ½ÇÉ«
    /// </summary>
    public class Role : IdGenEntity
    {
        [EntityProperty(Unique = true, NotNull = true)]
        public string Name { get; set; } = default!;

        [EntityProperty(MaxLength = 500, NotNull = true)]
        public string DisplayName { get; set; } = default!;

        [EntityProperty]
        public bool IsActivated { get; set; }

        [EntityProperty(MaxLength = 1024)]
        public string? Comment { get; set; }
    }


}