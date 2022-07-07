using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Database.DatabaseModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonModelPropertyAttribute : ModelPropertyAttribute
    {
        public JsonModelPropertyAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            Converter = typeof(JsonTypeConverter);
        }
    }
}
