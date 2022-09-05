using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreModelPropertyAttribute : Attribute
    {
    }
}
