using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HB.Infrastructure.Tencent.TCaptha
{
    /// <summary>
    /// 验证Http Header中的Captha
    /// </summary>
    public class TCapthcaCheckFilter : IAsyncActionFilter
    {
        private readonly ILogger<TCapthcaCheckFilter> _logger;
        private readonly ITCapthaClient _tCapthaClient;

        public TCapthcaCheckFilter(ILogger<TCapthcaCheckFilter> logger, ITCapthaClient tCapthaClient)
        {
            _logger = logger;
            _tCapthaClient = tCapthaClient;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                StringValues captcha = context.HttpContext.Request.Headers[ApiHeaderNames.Captcha];

                if (captcha.IsNullOrEmpty())
                {
                    OnError(context, ErrorCodes.CapthcaNotFound);
                    return;
                }

                string? captchaJson = captcha.First();

                TCaptchaResult? result = SerializeUtil.FromJson<TCaptchaResult>(captchaJson);

                if (result == null || !result.IsSuccessed)
                {
                    OnError(context, ErrorCodes.CapthcaError);
                    return;
                }

                bool verifyResult = await _tCapthaClient.VerifyTicketAsync(result.AppId, result.Ticket, result.Randstr, context!.HttpContext.GetIpAddress()).ConfigureAwait(false);

                if (!verifyResult)
                {
                    OnError(context, ErrorCodes.CapthcaError);
                    return;
                }

                await next().ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                OnError(context, ErrorCodes.CapthcaError);
                _logger.LogError(ex, "TCaptcha 验证执行失败");
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
