using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.DbModels
{
    public class PropertyOrderComparer : Comparer<PropertyInfo>
    {
        public override int Compare([AllowNull] PropertyInfo x, [AllowNull] PropertyInfo y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            var xAttr = x.GetCustomAttribute<DbModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(x);
            var yAttr = y.GetCustomAttribute<DbModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(y);


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

        private static DbModelPropertyAttribute? GetModelBasePropertyAttribute(PropertyInfo info)
        {
            return info.Name switch
            {
                nameof(TimestampGuidDbModel.Id) => new DbModelPropertyAttribute(0),
                nameof(TimestampDbModel.LastUser) => new DbModelPropertyAttribute(1),
                nameof(TimestampDbModel.Timestamp) => new DbModelPropertyAttribute(2),
                nameof(TimestampDbModel.Deleted) => new DbModelPropertyAttribute(3),
                //nameof(DbModel.CreateTime) => new DbModelPropertyAttribute(6),
                _ => null
            };
        }
    }
}
