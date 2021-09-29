using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

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
        [Route("GlobalException")]
        public IActionResult Exception()
        {
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature == null)
            {
                _logger.LogError("发生未知错误, GlobalExceptionController未记录Exception.");

                return new BadRequestObjectResult(ApiErrorCodes.UnKownServerError.AppendDetail("GlobalExceptionController未记录Exception"));
            }

            _logger.LogError(exceptionHandlerPathFeature.Error, "Error From ExceptionController");

            ErrorCode errorCode = exceptionHandlerPathFeature.Error switch
            {
                ErrorCodeException errorCodeException => errorCodeException.ErrorCode,
                Exception ex =>ApiErrorCodes.UnKownServerError.AppendDetail(ex.Message)
            };

            return new BadRequestObjectResult(errorCode)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
