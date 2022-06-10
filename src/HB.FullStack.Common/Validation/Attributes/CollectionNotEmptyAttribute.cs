using HB.FullStack.Common;

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel.DataAnnotations
{
    public sealed class CollectionNotEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is ICollection collection)
            {
                return collection.Count > 0;
            }

            return false;
        }
    }

    public sealed class CollectionMemeberValidatedAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            if (value is IEnumerable<ValidatableObject> vs)
            {
                foreach (ValidatableObject vo in vs)
                {
                    if(!vo.IsValid())
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
