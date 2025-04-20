using System.Globalization;

using System;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class LongId2Attribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public LongId2Attribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a valid Long Id";
            }
        }

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is long id && id > 0;
        }

    }
}
