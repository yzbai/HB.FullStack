using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Identity;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.WebApi.Filters
{
    public class UserActivityFilter : IAsyncActionFilter
    {
        private readonly ILogger<UserActivityFilter> _logger;
        private readonly IIdentityService _identityService;

        public UserActivityFilter(ILogger<UserActivityFilter> logger, IIdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                Guid? signInTokenId = context.HttpContext?.User?.GetSignInTokenId();
                Guid? userId = context.HttpContext?.User?.GetUserId();
                string? ip = context.HttpContext?.GetIpAddress();
                string? url = context.HttpContext?.Request?.GetDisplayUrl();
                string? httpMethod = context.HttpContext?.Request?.Method;
                string? arguments = SerializeUtil.TryToJson(context.ActionArguments);

                

                ActionExecutedContext? resultContext = await next().ConfigureAwait(false);

                int? resultStatusCode = null;
                ErrorCode? errorCode = null;
                string? resultType = null;

                if (resultContext?.Result != null)
                {
                    resultType = resultContext.Result.ToString();

                    if (resultContext.Result is BadRequestObjectResult badRequestObjectResult)
                    {
                        resultStatusCode = badRequestObjectResult.StatusCode;

                        if (badRequestObjectResult.Value is ErrorCode err)
                        {
                            errorCode = err;
                        }
                    }
                    else if (resultContext.Result is IStatusCodeActionResult statusCodeResult)
                    {
                        resultStatusCode = statusCodeResult.StatusCode;
                    }
                }

                _identityService.RecordUserActivityAsync(signInTokenId, userId, ip, url, httpMethod, arguments, resultStatusCode, resultType, errorCode).Fire();
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, "UserActivityFilter中捕捉到错误，不应该。请检查,是否有漏捕捉。{url}", context.HttpContext?.Request?.GetDisplayUrl());
                OnError(context);
            }
        }

        private static void OnError(ActionExecutingContext context)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(ApiErrorCodes.UserActivityFilterError);
            }
        }
    }
}
