

using System.ComponentModel.DataAnnotations;
using System;

namespace System.ComponentModel.DataAnnotations
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

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && StringValidationExtensions.IsMonth(text);
        }
    }
}