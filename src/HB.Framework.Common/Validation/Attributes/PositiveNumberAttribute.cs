#nullable enable

using System.ComponentModel.DataAnnotations;
using HB.Framework.Common.Validate;

namespace HB.Framework.Common.Validation.Attributes
{
    public sealed class PositiveNumberAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public PositiveNumberAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a Positive Number.";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null) { return CanBeNull; }

            return value is string text && ValidationMethods.IsPositiveNumber(text);
        }
    }
}