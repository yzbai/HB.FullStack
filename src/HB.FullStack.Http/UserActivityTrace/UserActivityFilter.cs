using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.WebApi.UserActivityTrace
{
    public class UserActivityFilter : IAsyncActionFilter
    {
        private readonly ILogger<UserActivityFilter> _logger;
        private readonly IUserActivityService _userActivityService;

        public UserActivityFilter(ILogger<UserActivityFilter> logger, IUserActivityService userActivityService)
        {
            _logger = logger;
            _userActivityService = userActivityService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                long? signInTokenId = context.HttpContext?.User?.GetSignInTokenId();
                long? userId = context.HttpContext?.User?.GetUserId();
                string? ip = context.HttpContext?.GetIpAddress();
                string? url = context.HttpContext?.Request?.GetDisplayUrl();
                string? httpMethod = context.HttpContext?.Request?.Method;
                string? arguments = SerializeUtil.TryToJson(context.ActionArguments);

                if (arguments != null && arguments.Length > UserActivity.MAX_ARGUMENTS_LENGTH)
                {
                    arguments = arguments.Substring(0, UserActivity.MAX_ARGUMENTS_LENGTH);
                }

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

                _userActivityService.RecordUserActivityAsync(signInTokenId, userId, ip, url, httpMethod, arguments, resultStatusCode, resultType, errorCode).Fire();
            }
            catch (Exception ex)
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
