using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Api
{
    public enum HttpMethodOverrideMode
    {
        None,
        Normal, //覆盖除了get 和 post之外的
        All //任何都覆盖
    }
}
