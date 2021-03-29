using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.UserActivityTrace
{
    public class UserActivityFilter : IAsyncActionFilter
    {
        private readonly ILogger<UserActivityFilter> _logger;

        public UserActivityFilter(ILogger<UserActivityFilter> logger)
        {
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                long? signInTokenId = context.HttpContext?.User?.GetSignInTokenId();

                await next().ConfigureAwait(false);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "");
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
