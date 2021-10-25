using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

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
        [HttpGet]
        public async Task<IActionResult> ExceptionAsync()
        {
            IExceptionHandlerPathFeature? exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (exceptionHandlerPathFeature == null)
            {
                _logger.LogError("IExceptionHandlerPathFeature = null");

                return new BadRequestObjectResult(WebApiErrorCodes.ExceptionHandlerPathFeatureNull);
            }
            
            string path = exceptionHandlerPathFeature.Path;
            //TODO: wait for .net 6
            //var routeValues = exceptionHandlerPathFeature.RouteValues;

            string? queryString = HttpContext.Request.QueryString.ToString();
            string? content = null;

            using (StreamReader bodyStream = new StreamReader(HttpContext.Request.Body))
            {
                bodyStream.BaseStream.Seek(0, SeekOrigin.Begin);
                content = await bodyStream.ReadToEndAsync().ConfigureAwait(false);
            }

            ErrorCode errorCode = WebApiErrorCodes.ServerUnKownNonErrorCodeError;

            if(exceptionHandlerPathFeature.Error is ErrorCode2Exception errorCodeException)
            {
                errorCode = errorCodeException.ErrorCode;
            }

            _logger.LogGlobalException(path, null, queryString, content, errorCode, exceptionHandlerPathFeature.Error);

            return new BadRequestObjectResult(errorCode)
            {
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}
