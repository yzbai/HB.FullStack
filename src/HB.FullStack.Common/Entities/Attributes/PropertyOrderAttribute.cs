using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace HB.FullStack.Common.Entities.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyOrderAttribute : Attribute
    {
        public int Order { get; private set; }

        public PropertyOrderAttribute([CallerLineNumber] int order = 0)
        {
            Order = order;
        }
    }
}
