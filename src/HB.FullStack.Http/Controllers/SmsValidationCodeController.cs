using System.ComponentModel.DataAnnotations;

using AsyncAwaitBestPractices;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Services;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [Route($"api/[controller]")]
    [ApiController]
    public class SmsValidationCodeController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ISmsService _smsService;

        public SmsValidationCodeController(ILogger<SmsValidationCodeController> logger, ISmsService aliyunSmsService)
        {
            _logger = logger;
            _smsService = aliyunSmsService;
        }

        [AllowAnonymous]
        [HttpGet(SharedNames.Conditions.ByMobile)]
        [ProducesResponseType(200)]
        //TODO: Remove this later
        //#if !DEBUG
        //TODO: 将CapthaFilter 抽象出来，不能只依赖腾讯一家
        //[ServiceFilter(typeof(CapthcaCheckFilter))]
        //#endif
        public IActionResult GetByMobile([FromQuery][Mobile] string mobile)
        {
            //_smsService.SendValidationCodeAsync(mobile)
            //    .SafeFireAndForget(ex =>
            //    {
            //        _logger.LogCritical(ex, "短信服务发生故障，请尽快检查!");
            //    });

            //TODO: 调查这个

            //TODO: 同一用户的罚时操作？比如同一手机号，连续请求5次以上？

#if DEBUG
            _smsService.SendValidationCodeAsync(mobile, "1111", int.MaxValue).SafeFireAndForget();

            return Ok(new SmsValidationCodeRes { Length = 4 });
#else

            int smsCodeLength = _smsService.SendValidationCode(mobile);

            return Ok(new SmsValidationCodeRes { Length = smsCodeLength });
#endif
        }
    }
}
