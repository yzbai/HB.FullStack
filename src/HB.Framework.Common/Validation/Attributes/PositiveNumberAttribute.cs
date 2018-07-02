using HB.Framework.Common.Validate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.Common.Validation.Attributes
{
    public class PositiveNumberAttribute : ValidationAttribute
    {
        public PositiveNumberAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "xxx";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null) { return true; }

            string text = value as string;

            return text != null && ValidationMethods.IsPositiveNumber(text);
        }
    }
}
