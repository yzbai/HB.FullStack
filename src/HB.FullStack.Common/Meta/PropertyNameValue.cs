using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Meta
{
    public class PropertyNameValue
    {
        public string Name { get; set; } = null!;

        public object? Value { get; set; }

        public PropertyNameValue(string propertyName, object? value)
        {
            Name = propertyName;
            Value = value;
        }
    }
}
