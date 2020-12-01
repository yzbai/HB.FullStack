using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.Component.Identity;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Server;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.Filters
{
    public class CheckSmsCodeFilter : IAsyncActionFilter
    {
        private readonly ILogger _logger;
        private readonly ISmsService _smsService;
        private readonly IIdentityService _identityService;

        public CheckSmsCodeFilter(ILogger<CheckSmsCodeFilter> logger, ISmsService smsService, IIdentityService identityService)
        {
            _logger = logger;
            _smsService = smsService;
            _identityService = identityService;
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

                    if (!_smsService.Validate(mobile!, smsCode!))
                    {
                        _identityService.OnLoginBySmsFailedAsync(mobile).Fire();
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
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
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
