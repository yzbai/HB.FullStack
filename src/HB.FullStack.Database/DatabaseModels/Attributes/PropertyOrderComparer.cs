using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.Database.DatabaseModels
{
    public class PropertyOrderComparer : Comparer<PropertyInfo>
    {
        public override int Compare([AllowNull] PropertyInfo x, [AllowNull] PropertyInfo y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            var xAttr = x.GetCustomAttribute<ModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(x);
            var yAttr = y.GetCustomAttribute<ModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(y);


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
                return xAttr.PropertyOrder - yAttr.PropertyOrder;
            }

        }

        private static ModelPropertyAttribute? GetModelBasePropertyAttribute(PropertyInfo info)
        {
            return info.Name switch
            {
                nameof(LongIdModel.Id) => new ModelPropertyAttribute(0),
                
                nameof(Model.Version) => new ModelPropertyAttribute(2),
                nameof(Model.LastUser) => new ModelPropertyAttribute(3),
                nameof(Model.LastTime) => new ModelPropertyAttribute(4),
                nameof(Model.Deleted) => new ModelPropertyAttribute(5),
                nameof(Model.CreateTime) => new ModelPropertyAttribute(6),
                _ => null
            };
        }
    }
}
