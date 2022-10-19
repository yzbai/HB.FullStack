using HB.FullStack.Common;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class ValidatedObjectAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public ValidatedObjectAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a Valid";
            }
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is ValidatableObject validatableObject && validatableObject.IsValid();
        }
    }
}