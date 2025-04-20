using System.Collections.Generic;
using System.Linq;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class CollectionNotNullOrEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            return value is IEnumerable<object> vs && vs.Any();
        }
    }
}
