using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace HB.FullStack.Server
{
    public class ResourceController<T> : BaseController where T : ApiResource
    {
        protected OkObjectResult Resource(T? res)
        {
            if(res == null)
            {
                return base.Ok(Array.Empty<T>());
            }

            return base.Ok(new T[] { res });
        }

        protected OkObjectResult Resource(IEnumerable<T> resources)
        {
            return base.Ok(resources);
        }

        protected OkObjectResult NewlyAdded(IEnumerable<long> ids)
        {
            return base.Ok(ids);
        }

        public new OkObjectResult Ok([ActionResultObjectValue] object value)
        {
            throw new NotSupportedException("使用Resource或者NewlyAdded");
        }
    }
}
