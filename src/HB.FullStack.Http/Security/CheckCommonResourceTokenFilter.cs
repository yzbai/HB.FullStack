using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.WebLib.Security;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HB.FullStack.Server.WebLib.Filters
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
                if (await _securityService.NeedPublicResourceTokenAsync(context).ConfigureAwait(false))
                {
                    StringValues token = context!.HttpContext.Request.Headers[SharedNames.ApiHeaders.CommonResourceToken];

                    if (token.IsNullOrEmpty())
                    {
                        OnError(context, ErrorCodes.CommonResourceTokenNeeded);
                        return;
                    }

                    string? crt = token.First();

                    if (!_commonResTokenService.TryCheckToken(crt, out string? _))
                    {
                        OnError(context, ErrorCodes.CommonResourceTokenError);
                        return;
                    }
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
