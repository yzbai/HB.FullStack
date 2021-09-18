using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class NoEmptyGuidAttribute : ValidationAttribute
    {
        public NoEmptyGuidAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Require a not empty Guid";
            }
        }

        public override bool IsValid(object? value)
        {
            return value != null && ((Guid)value) != Guid.Empty;
        }

    }
}
