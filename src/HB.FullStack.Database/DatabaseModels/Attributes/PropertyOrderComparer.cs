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

            var xAttr = x.GetCustomAttribute<DatabaseModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(x);
            var yAttr = y.GetCustomAttribute<DatabaseModelPropertyAttribute>(true) ?? GetModelBasePropertyAttribute(y);


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

        private static DatabaseModelPropertyAttribute? GetModelBasePropertyAttribute(PropertyInfo info)
        {
            return info.Name switch
            {
                nameof(GuidDatabaseModel.Id) => new DatabaseModelPropertyAttribute(0),
                nameof(ServerDatabaseModel.LastUser) => new DatabaseModelPropertyAttribute(1),
                nameof(ServerDatabaseModel.Timestamp) => new DatabaseModelPropertyAttribute(2),
                nameof(ServerDatabaseModel.Deleted) => new DatabaseModelPropertyAttribute(3),
                //nameof(DatabaseModel.CreateTime) => new DatabaseModelPropertyAttribute(6),
                _ => null
            };
        }
    }
}
