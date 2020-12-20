using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HB.FullStack.Server
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode, string? message = null)
        {
            return BadRequest(new ApiError(errorCode, message ?? errorCode.ToString()));
        }
    }
}
