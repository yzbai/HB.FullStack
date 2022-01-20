

using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class MobileAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public MobileAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = "Not a Valid Mobile";
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && ValidationMethods.IsMobilePhone(text);
        }
    }
}