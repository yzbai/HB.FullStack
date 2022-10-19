using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.PropertyTrackable
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AddtionalPropertyAttribute : Attribute
    {

    }
}
