using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Database.DBModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreModelPropertyAttribute : Attribute
    {
    }
}
