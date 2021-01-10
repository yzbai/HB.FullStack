using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Def;

namespace HB.FullStack.Database.Def
{
    public class PropertyOrderComparer : Comparer<PropertyInfo>
    {
        public override int Compare([AllowNull] PropertyInfo x, [AllowNull] PropertyInfo y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            var xAttr = x.GetCustomAttribute<EntityPropertyAttribute>(true) ?? GetEntityBasePropertyAttribute(x);
            var yAttr = y.GetCustomAttribute<EntityPropertyAttribute>(true) ?? GetEntityBasePropertyAttribute(y);


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

        private static EntityPropertyAttribute? GetEntityBasePropertyAttribute(PropertyInfo info)
        {
            return info.Name switch
            {
                nameof(IdGenEntity.Id) => new EntityPropertyAttribute(0),
                nameof(GuidEntity.Guid) => new EntityPropertyAttribute(1),
                nameof(Entity.Version) => new EntityPropertyAttribute(2),
                nameof(Entity.LastUser) => new EntityPropertyAttribute(3),
                nameof(Entity.LastTime) => new EntityPropertyAttribute(4),
                nameof(Entity.Deleted) => new EntityPropertyAttribute(5),
                nameof(Entity.CreateTime) => new EntityPropertyAttribute(6),
                _ => null
            };
        }
    }
}
