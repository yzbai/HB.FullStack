using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class CollectionMemeberValidatedAttribute : ValidationAttribute
    {
        public bool CanBeNullOrEmpty { get; set; } = true;

        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return CanBeNullOrEmpty;
            }

            if (value is IEnumerable<ValidatableObject> vs)
            {
                if (!vs.Any())
                {
                    return CanBeNullOrEmpty;
                }

                foreach (ValidatableObject vo in vs)
                {
                    if (!vo.IsValid())
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
