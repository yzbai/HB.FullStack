using HB.Framework.Common.Api;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HB.Framework.Http
{
    public class ExtendedController : ControllerBase
    {
        protected BadRequestObjectResult Error(ServerErrorCode errorCode, string? message = null)
        {
            return BadRequest(new ApiError(errorCode, message ?? errorCode.ToString()));
        }
    }
}
