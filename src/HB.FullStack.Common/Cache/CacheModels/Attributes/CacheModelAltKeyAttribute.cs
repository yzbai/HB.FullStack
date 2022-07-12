using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 辅助key，可以有多个.通过主key或者辅助key均可获取cache
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CacheModelAltKeyAttribute : Attribute
    {

    }
}
