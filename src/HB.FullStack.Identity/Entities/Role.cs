using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Database.Def;

namespace HB.FullStack.Identity.Entities
{
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

        public void Update(string name, string displayName, bool isActivated, string? comment)
        {
            Name = name;
            DisplayName = displayName;
            IsActivated = isActivated;
            Comment = comment;
        }
    }


}