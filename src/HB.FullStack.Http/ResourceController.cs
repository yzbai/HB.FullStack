using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Server
{
    public class ResourceController<T> : BaseController where T : ApiResource
    {

    }
}
