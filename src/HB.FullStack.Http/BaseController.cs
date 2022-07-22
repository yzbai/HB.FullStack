using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.WebApi
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode)
        {
            return BadRequest(errorCode);
        }
    }

    public class ModelController<TModel> : BaseController where TModel : IModel
    {

    }
}
