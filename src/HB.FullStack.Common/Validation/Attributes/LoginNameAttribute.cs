#nullable enable

using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class LoginNameAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public LoginNameAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a valid LoginName";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && text.Length < ValidationSettings.LoginNameMaxLength && ValidationMethods.IsLoginName(text);
        }
    }
}