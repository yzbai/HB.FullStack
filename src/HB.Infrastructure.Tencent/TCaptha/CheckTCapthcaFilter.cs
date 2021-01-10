using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.Tencent.TCaptha
{
    public class CheckTCapthcaFilter : IAsyncActionFilter
    {
        private readonly ILogger<CheckTCapthcaFilter> _logger;
        private readonly ITCapthaClient _tCapthaClient;

        public CheckTCapthcaFilter(ILogger<CheckTCapthcaFilter> logger, ITCapthaClient tCapthaClient)
        {
            _logger = logger;
            _tCapthaClient = tCapthaClient;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                KeyValuePair<string, object>? firstArgument = context?.ActionArguments?.FirstOrDefault();
                object? firstArgumentValue = firstArgument.HasValue ? firstArgument.Value.Value : null;

                if (firstArgumentValue is not ApiRequest apiRequest)
                {
                    OnError(context, ApiErrorCode.ApiPublicResourceTokenNeeded);
                    return;
                }
                if (apiRequest.PublicResourceToken.IsNullOrEmpty())
                {
                    OnError(context, ApiErrorCode.ApiPublicResourceTokenNeeded);
                    return;
                }

                TCaptchaResult? result = SerializeUtil.FromJson<TCaptchaResult>(apiRequest.PublicResourceToken);

                if (result == null || !result.IsSuccessed)
                {
                    OnError(context, ApiErrorCode.ApiPublicResourceTokenNeeded);
                    return;
                }

                bool verifyResult = await _tCapthaClient.VerifyTicketAsync(result.AppId, result.Ticket, result.Randstr, context!.HttpContext.GetIpAddress()).ConfigureAwait(false);

                if (!verifyResult)
                {
                    OnError(context, ApiErrorCode.ApiPublicResourceTokenError);
                    return;
                }

                await next().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError(context, ApiErrorCode.ApiPublicResourceTokenNeeded);
                _logger.LogError(ex, "TCaptcha 验证执行失败");
            }
        }

        private static void OnError(ActionExecutingContext? context, ApiErrorCode error)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(new ApiError(error));
            }
        }
    }
}
