using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.ApiClient
{
    public class ApiResourceDef
    {
        public string ResName { get; internal set; } = null!;

        public IDictionary<string, ApiResourcePropertyDef> PropertyDefs { get; } = new Dictionary<string, ApiResourcePropertyDef>();

        public ApiResourcePropertyDef? GetPropertyDef(string propertyName)
        {
            if (PropertyDefs.TryGetValue(propertyName, out ApiResourcePropertyDef? propertyDef))
            {
                return propertyDef;
            }

            return null;
        }
    }
}
