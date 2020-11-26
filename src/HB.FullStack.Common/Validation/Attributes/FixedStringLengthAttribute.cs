using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Validation.Attributes
{
    public sealed class FixedStringLengthAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        private readonly int _length;

        public FixedStringLengthAttribute(int length)
        {
            _length = length;

            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a FiexedString";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && text.Length == _length;
        }
    }
}