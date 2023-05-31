using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

using AsyncAwaitBestPractices;

using HB.FullStack.Server.Identity;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Filters
{
    public class UserActivityFilter<TId> : IAsyncActionFilter
    {
        private readonly ILogger<UserActivityFilter<TId>> _logger;
        private readonly IIdentityService<TId> _identityService;

        public UserActivityFilter(ILogger<UserActivityFilter<TId>> logger, IIdentityService<TId> identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                ClaimsPrincipal? principal = context.HttpContext?.User;

                TId? userId =  principal!= null ? principal.GetUserId<TId>() : default;
                TId? signInCredentialId = principal != null ? principal.GetTokenCredentialId<TId>() : default;
                string? ip = context.HttpContext?.GetIpAddress();
                string? url = context.HttpContext?.Request?.GetDisplayUrl();
                string? httpMethod = context.HttpContext?.Request?.Method;

                SerializeUtil.TryToJson(context.ActionArguments, out string? arguments);

                ActionExecutedContext? resultContext = await next().ConfigureAwait(true);

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

                _identityService.RecordUserActivityAsync(signInCredentialId, userId, ip, url, httpMethod, arguments, resultStatusCode, resultType, errorCode)
                    .SafeFireAndForget(ex =>
                    {
                        //TODO:错误处理？
                        _logger.LogError(ex, $"{nameof(_identityService.RecordUserActivityAsync)} 在 {nameof(UserActivityFilter<TId>)} 中调用出错。");
                    });
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, "UserActivityFilter中捕捉到错误，不应该。请检查,是否有漏捕捉。{RequestUrl}", context.HttpContext?.Request?.GetDisplayUrl());
                OnError(context);
            }
        }

        private static void OnError(ActionExecutingContext context)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(ErrorCodes.UserActivityFilterError);
            }
        }
    }
}
