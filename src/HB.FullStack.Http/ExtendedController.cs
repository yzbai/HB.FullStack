using HB.FullStack.Common.Api;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HB.FullStack.Server
{
    public class ExtendedController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode, string? message = null)
        {
            return BadRequest(new ApiError(errorCode, message ?? errorCode.ToString()));
        }
    }
}
