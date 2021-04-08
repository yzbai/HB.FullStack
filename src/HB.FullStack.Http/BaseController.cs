using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;
using System;

namespace HB.FullStack.Server
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode)
        {
            return BadRequest(errorCode);
        }
    }
}
