#nullable enable

using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class SmsCodeAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public SmsCodeAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a SmsCode.";
            }
        }

        public override bool IsValid(object? value)
        {
            if (value == null) { return CanBeNull; }

            return value is string text && ValidationMethods.IsSmsCode(text, null);
        }
    }
}