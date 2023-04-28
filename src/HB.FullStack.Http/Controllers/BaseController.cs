using System;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib.Controllers
{
    public class BaseController : ControllerBase
    {
        protected BadRequestObjectResult Error(ErrorCode errorCode)
        {
            return BadRequest(errorCode);
        }

        protected BadRequestObjectResult Error(ErrorCode errorCode, string description)
        {
            return BadRequest(new ErrorCode(errorCode.Code, description));
        }
    }
}
