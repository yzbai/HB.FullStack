using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Entities.Attributes;

namespace HB.FullStack.Common.Entities
{
    public class PropertyOrderComparer : Comparer<PropertyInfo>
    {
        public override int Compare([AllowNull] PropertyInfo x, [AllowNull] PropertyInfo y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            var xAttr = x.GetCustomAttribute<PropertyOrderAttribute>(true);
            var yAttr = y.GetCustomAttribute<PropertyOrderAttribute>(true);

            if (xAttr == null && yAttr == null)
            {
                return 0;
            }
            else if (xAttr == null)
            {
                return 1;
            }
            else if (yAttr == null)
            {
                return -1;
            }
            else
            {
                return xAttr.Order - yAttr.Order;
            }

        }
    }
}
