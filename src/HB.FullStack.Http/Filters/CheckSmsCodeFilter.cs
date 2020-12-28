using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.Server;
using HB.FullStack.Identity;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Filters
{
    public class CheckSmsCodeFilter : IAsyncActionFilter
    {
        private readonly ILogger _logger;
        private readonly ISmsServerService _smsService;
        private readonly IAuthorizationService _authorizationService;

        public CheckSmsCodeFilter(ILogger<CheckSmsCodeFilter> logger, ISmsServerService smsService, IAuthorizationService authorizationService)
        {
            _logger = logger;
            _smsService = smsService;
            _authorizationService = authorizationService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                KeyValuePair<string, object>? firstArgument = context?.ActionArguments?.FirstOrDefault();
                object? firstArgumentValue = firstArgument.HasValue ? firstArgument.Value.Value : null;

                if (firstArgumentValue is ApiRequest apiRequest)
                {
                    Type apiRequestType = apiRequest.GetType();

                    string? smsCode = apiRequestType.GetProperty(ClientNames.SmsCode)?.GetValue(apiRequest)?.ToString();
                    string? mobile = apiRequestType.GetProperty(ClientNames.Mobile)?.GetValue(apiRequest)?.ToString();

                    if (smsCode.IsNullOrEmpty() || mobile.IsNullOrEmpty())
                    {
                        OnError(context);
                        return;
                    }

                    if (!await _smsService.ValidateAsync(mobile!, smsCode!).ConfigureAwait(false))
                    {
                        _authorizationService.OnSignInFailedBySmsAsync(mobile!, apiRequest.DeviceInfos.Name).Fire();
                        OnError(context);
                        return;
                    }
                }
                else
                {
                    OnError(context);
                    return;
                }

                await next().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                OnError(context);
                _logger.LogError(ex, "SmsCode 验证失败");
            }
        }

        private static void OnError(ActionExecutingContext? context)
        {
            if (context != null)
            {
                context.Result = new BadRequestObjectResult(new ApiError(ErrorCode.ApiSmsCodeInvalid));
            }
        }
    }
}
