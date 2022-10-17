using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Cache
{

    public class CacheModelDef : ModelDef
    {
        public PropertyInfo KeyProperty { get; internal set; } = null!;

        public IList<PropertyInfo> AltKeyProperties { get; private set; } = new List<PropertyInfo>();

        public string Name { get; internal set; } = null!;

        /// <summary>
        /// 多久不经常用就消失掉
        /// </summary>
        public TimeSpan? SlidingTime { get; set; }

        /// <summary>
        /// 最多存在多久
        /// </summary>
        public TimeSpan? AbsoluteTimeRelativeToNow { get; set; }

        public string? CacheInstanceName { get; set; }

        public override ModelPropertyDef? GetPropertyDef(string propertyName)
        {
            throw new NotImplementedException();
        }

        //public bool IsBatchEnabled { get; set; }
    }
}
