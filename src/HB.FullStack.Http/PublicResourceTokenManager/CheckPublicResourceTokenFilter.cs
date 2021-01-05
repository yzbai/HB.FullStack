using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using HB.FullStack.Common.Api;
using Microsoft.Extensions.Logging;
using HB.FullStack.Server.Security;

namespace HB.FullStack.Server.Filters
{
    public class CheckPublicResourceTokenFilter : IAsyncActionFilter
    {
        private readonly ILogger<CheckPublicResourceTokenFilter> _logger;
        private readonly ISecurityService _securityService;
        private readonly IPublicResourceTokenManager _publicResourceTokenManager;

        public CheckPublicResourceTokenFilter(ILogger<CheckPublicResourceTokenFilter> logger, ISecurityService securityService, IPublicResourceTokenManager publicResourceTokenManager)
        {
            _logger = logger;
            _securityService = securityService;
            _publicResourceTokenManager = publicResourceTokenManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                KeyValuePair<string, object>? firstArgument = context?.ActionArguments?.FirstOrDefault();
                object? firstArgumentValue = firstArgument.HasValue ? firstArgument.Value.Value : null;

                if (firstArgumentValue is ApiRequest apiRequest)
                {
                    if (await _securityService.NeedPublicResourceTokenAsync(apiRequest).ConfigureAwait(false))
                    {
                        if (apiRequest.PublicResourceToken.IsNullOrEmpty())
                        {
                            OnError(context, ErrorCode.ApiPublicResourceTokenNeeded);
                            return;
                        }

                        if (!await _publicResourceTokenManager.CheckTokenAsync(apiRequest.PublicResourceToken).ConfigureAwait(false))
                        {
                            OnError(context, ErrorCode.ApiPublicResourceTokenError);
                            return;
                        }
                    }
                }
                else
                {
                    OnError(context, ErrorCode.ApiPublicResourceTokenNeeded);
                    return;
                }

                await next().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError(context, ErrorCode.ApiPublicResourceTokenNeeded);
                _logger.LogError(ex, "PublicResourceToken 验证失败");
            }
        }

        private static void OnError(ActionExecutingContext? context, ErrorCode error)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(new ApiError(error));
            }
        }
    }
}
