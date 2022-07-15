using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.DBModels;

namespace HB.FullStack.Database.DBModels
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class JsonModelPropertyAttribute : DBModelPropertyAttribute
    {
        public JsonModelPropertyAttribute([CallerLineNumber] int propertyOrder = 0) : base(propertyOrder)
        {
            Converter = typeof(JsonTypeConverter);
        }
    }
}
