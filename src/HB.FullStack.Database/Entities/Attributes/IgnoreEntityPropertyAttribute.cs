using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Database.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreEntityPropertyAttribute : Attribute
    {
    }
}
