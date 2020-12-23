#nullable enable

using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
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