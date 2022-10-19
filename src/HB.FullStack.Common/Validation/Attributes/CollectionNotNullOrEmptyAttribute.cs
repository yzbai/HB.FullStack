using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;

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
