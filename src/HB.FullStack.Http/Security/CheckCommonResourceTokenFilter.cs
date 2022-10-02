using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.WebApi.Security;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HB.FullStack.WebApi.Filters
{
    public class CheckCommonResourceTokenFilter : IAsyncActionFilter
    {
        private readonly ILogger<CheckCommonResourceTokenFilter> _logger;
        private readonly ISecurityService _securityService;
        private readonly ICommonResourceTokenService _commonResTokenService;

        public CheckCommonResourceTokenFilter(ILogger<CheckCommonResourceTokenFilter> logger, ISecurityService securityService, ICommonResourceTokenService publicResourceTokenManager)
        {
            _logger = logger;
            _securityService = securityService;
            _commonResTokenService = publicResourceTokenManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                KeyValuePair<string, object?>? firstArgument = context?.ActionArguments?.FirstOrDefault();
                object? firstArgumentValue = firstArgument.HasValue ? firstArgument.Value.Value : null;

                if (firstArgumentValue is ApiRequest apiRequest)
                {
                    if (await _securityService.NeedPublicResourceTokenAsync(apiRequest).ConfigureAwait(false))
                    {
                        StringValues token = context!.HttpContext.Request.Headers[ApiHeaderNames.CommonResourceToken];

                        if (token.IsNullOrEmpty())
                        {
                            OnError(context, ErrorCodes.CommonResourceTokenNeeded);
                            return;
                        }

                        string crt = token.First();

                        if (!_commonResTokenService.TryCheckToken(crt, out string? _))
                        {
                            OnError(context, ErrorCodes.CommonResourceTokenError);
                            return;
                        }
                    }
                }
                else
                {
                    OnError(context, ErrorCodes.CommonResourceTokenNeeded);
                    return;
                }

                await next().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError(context, ErrorCodes.CommonResourceTokenNeeded);
                _logger.LogError(ex, "CommonResourceToken 验证失败");
            }
        }

        private static void OnError(ActionExecutingContext? context, ErrorCode error)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(error);
            }
        }
    }
}
