using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.UserActivityTrace
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
                string? arguments = SerializeUtil.TryToJson(context.ActionArguments);

                await next().ConfigureAwait(false);

                int? resultStatusCode = context.HttpContext?.Response?.StatusCode;

                _userActivityService.RecordUserActivityAsync(signInTokenId, userId, ip, url, arguments, resultStatusCode).Fire();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "UserActivityFilter中捕捉到错误，不应该。请检查,是否有漏捕捉。Url:{url}", context.HttpContext?.Request?.GetDisplayUrl());
                OnError(context);
            }
        }

        private static void OnError(ActionExecutingContext context)
        {
            if(context != null)
            {
                context.Result = new BadRequestObjectResult(new ApiError(ApiErrorCode.UserActivityFilterError));
            }
        }
    }
}
