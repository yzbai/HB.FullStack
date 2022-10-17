using System.Collections.Generic;

namespace HB.FullStack.Common.Models
{
    public class PlainModelDef : ModelDef
    {
        public IDictionary<string, PlainModelPropertyDef> PropertyDict { get; } = new Dictionary<string, PlainModelPropertyDef>();

        public override ModelPropertyDef? GetPropertyDef(string propertyName)
        {
            if (PropertyDict.TryGetValue(propertyName, out PlainModelPropertyDef? propertyDef))
            {
                return propertyDef;
            }

            return null;
        }
    }
}
