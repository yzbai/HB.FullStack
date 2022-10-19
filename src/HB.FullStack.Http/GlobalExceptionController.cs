using System;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.WebApi
{
    [ApiController]
    public class GlobalExceptionController : BaseController
    {
        private readonly ILogger _logger;

        public GlobalExceptionController(ILogger<GlobalExceptionController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet("GlobalException")]
        public IActionResult ExceptionAsync()
        {
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature == null)
            {
                //TODO: 完善， 记录请求Request。

                _logger.LogCritical("IExceptionHandlerPathFeature = null");

                return new BadRequestObjectResult(ErrorCodes.ServerInternalError);
            }

            string path = exceptionHandlerPathFeature.Path;

            //TODO: Do we need so mutch detail ?

            //RouteValueDictionary? routeValues = exceptionHandlerPathFeature.RouteValues;

            //string? queryString = HttpContext.Request.QueryString.ToString();
            //string? content = null;

            //using (StreamReader bodyStream = new StreamReader(HttpContext.Request.Body))
            //{
            //    bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
            //    content = await bodyStream.ReadToEndAsync().ConfigureAwait(false);
            //}

            ErrorCode errorCode = ErrorCodes.ServerInternalError;

            if (exceptionHandlerPathFeature.Error is ErrorCodeException errorCodeException)
            {
                errorCode = errorCodeException.ErrorCode;
            }

            _logger.LogError(exceptionHandlerPathFeature.Error, "GlobalExceptionController捕捉异常：RequestPath:{RequestPath}", path);

            return new BadRequestObjectResult(errorCode)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
