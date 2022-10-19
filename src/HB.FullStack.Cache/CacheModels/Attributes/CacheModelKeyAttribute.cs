using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 这时主key，一般标记在Id上。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CacheModelKeyAttribute : Attribute
    {

    }
}
