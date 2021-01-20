#nullable enable

using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class NickNameAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public NickNameAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a valid NickName.";
            }
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && text.Length < ValidationSettings.LoginNameMaxLength && ValidationMethods.IsNickName(text);
        }
    }
}