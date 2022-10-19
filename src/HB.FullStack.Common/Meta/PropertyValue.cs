using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Meta
{
    public class PropertyValue
    {
        public string PropertyName { get; set; } = null!;

        public object? Value { get; set; }

        public PropertyValue(string propertyName, object? value)
        {
            PropertyName = propertyName;
            Value = value;
        }
    }
}
