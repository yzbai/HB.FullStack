using System.ComponentModel.DataAnnotations;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class FixedStringLengthAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;


        public FixedStringLengthAttribute(int length)
        {
            Length = length;

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a FiexedString";
            }
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && text.Length == Length;
        }

        public int Length { get; }
    }
}