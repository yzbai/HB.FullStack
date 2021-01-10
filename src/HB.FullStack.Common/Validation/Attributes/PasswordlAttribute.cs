#nullable enable

using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class PasswordAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public PasswordAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = "Not a valid Password.";
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && ValidationMethods.IsPassword(text);
        }
    }
}