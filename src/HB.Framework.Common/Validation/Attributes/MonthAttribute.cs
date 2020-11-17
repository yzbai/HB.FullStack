#nullable enable

using System.ComponentModel.DataAnnotations;
using HB.Framework.Common.Validate;

namespace HB.Framework.Common.Validation.Attributes
{
    public sealed class MonthAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public MonthAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a Valid Month.";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && ValidationMethods.IsMonth(text);
        }
    }
}