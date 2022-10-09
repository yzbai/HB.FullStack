using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.DbModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonModelPropertyAttribute : DbModelPropertyAttribute
    {
        public JsonModelPropertyAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            Converter = typeof(JsonDbPropertyConverter);
        }
    }
}
