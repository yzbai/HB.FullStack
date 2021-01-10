using HB.FullStack.Common.Api;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.Server
{
    [ApiController]
    public class ExceptionController : BaseController
    {
        private readonly ILogger _logger;

        public ExceptionController(ILogger<ExceptionController> logger)
        {
            _logger = logger;
        }

        [Route("exception")]
        public IActionResult Exception()
        {
            IExceptionHandlerFeature exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>();

            _logger.LogError(exceptionHandlerFeature.Error, "Error From ExceptionController");

            ApiError apiErrorResponse = new ApiError(ApiErrorCode.FromExceptionController, exceptionHandlerFeature.Error.Message);

            return new BadRequestObjectResult(apiErrorResponse)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
