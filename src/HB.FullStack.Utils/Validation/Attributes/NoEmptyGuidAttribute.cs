using System;
using System.Collections;
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
            if (value is IEnumerable cols)
            {
                foreach (object? obj in cols)
                {
                    if (!IsObjectValid(obj))
                    {
                        return false;
                    }
                }

                return true;
            }

            return IsObjectValid(value);
        }

        public static bool IsObjectValid(object? value)
        {
            Guid? guid = value as Guid?;

            return guid != null && guid != Guid.Empty;
        }
    }
}
