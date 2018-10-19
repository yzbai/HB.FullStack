using HB.Framework.Common.Validate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HB.Framework.Common.Validation.Attributes
{
    public sealed class MonthAttribute : ValidationAttribute
    {
        public MonthAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "xxxxx";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            return value is string text && ValidationMethods.IsMonth(text);
        }
    }
}
