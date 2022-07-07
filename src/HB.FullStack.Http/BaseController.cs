using HB.FullStack.Common;
using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;
using System;

namespace HB.FullStack.WebApi
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode)
        {
            return BadRequest(errorCode);
        }
    }

    public class BaseController<TModel> : BaseController where TModel : IModel
    {
        //protected OkObjectResult Ok(T? res)
        //{
        //    if(res == null)
        //    {
        //        return base.Ok(Array.Empty<T>());
        //    }

        //    return base.Ok(new T[] { res });
        //}

        //protected OkObjectResult Ok(IEnumerable<T> resources)
        //{
        //    return base.Ok(resources);
        //}

        //protected OkObjectResult NewlyAdded(IEnumerable<long> ids)
        //{
        //    return base.Ok(ids);
        //}

        //public new OkObjectResult Ok([ActionResultObjectValue] object _)
        //{
        //    throw new NotSupportedException("使用Resource或者NewlyAdded");
        //}
    }
}
