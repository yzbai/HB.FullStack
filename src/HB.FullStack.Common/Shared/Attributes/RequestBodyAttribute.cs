using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Shared.Attributes
{
    /// <summary>
    /// 同一个Request中只能有一个RequestBody
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RequestBodyAttribute : Attribute
    {

    }
}
