using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Identity.Entities
{
    public class Role : ModelObject
    {
        public string Name { get; set; } = default!;

        public string DisplayName { get; set; } = default!;

        public bool IsActivated { get; set; }

        public string? Comment { get; set; }
    }


}