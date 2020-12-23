#nullable enable

using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Validate;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class DayAttribute : ValidationAttribute
    {
        public bool CanBeNull { get; set; } = true;

        public DayAttribute()
        {
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = "Not a valid Day.";
            }
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return CanBeNull;
            }

            return value is string text && ValidationMethods.IsDay(text);
        }
    }
}