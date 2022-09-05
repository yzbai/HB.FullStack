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

            var xAttr = x.GetCustomAttribute<DBModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(x);
            var yAttr = y.GetCustomAttribute<DBModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(y);


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

        private static DBModelPropertyAttribute? GetModelBasePropertyAttribute(PropertyInfo info)
        {
            return info.Name switch
            {
                nameof(TimestampGuidDbModel.Id) => new DBModelPropertyAttribute(0),
                nameof(TimestampDbModel.LastUser) => new DBModelPropertyAttribute(1),
                nameof(TimestampDbModel.Timestamp) => new DBModelPropertyAttribute(2),
                nameof(TimestampDbModel.Deleted) => new DBModelPropertyAttribute(3),
                //nameof(DbModel.CreateTime) => new DBModelPropertyAttribute(6),
                _ => null
            };
        }
    }
}
