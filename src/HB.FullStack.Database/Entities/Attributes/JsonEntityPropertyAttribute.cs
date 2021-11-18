using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.Entities
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonEntityPropertyAttribute : EntityPropertyAttribute
    {
        public JsonEntityPropertyAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            Converter = typeof(JsonTypeConverter);
        }
    }
}
