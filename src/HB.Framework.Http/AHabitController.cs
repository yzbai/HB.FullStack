using HB.Framework.Common.Api;
using Microsoft.AspNetCore.Mvc;

namespace HB.Framework.Http
{
    public class ExtendedController : ControllerBase
    {
        protected BadRequestObjectResult Error(ApiError errorCode, string message = null)
        {
            return BadRequest(new ApiErrorResponse(errorCode, message));
        }
    }
}
