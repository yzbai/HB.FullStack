using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using HB.FullStack.Common.Server;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Web.Controllers
{
    [Route($"api/[controller]")]
    [ApiController]
    public class SmsValidationCodeController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ISmsServerService _smsService;

        public SmsValidationCodeController(ILogger<SmsValidationCodeController> logger, ISmsServerService aliyunSmsService)
        {
            _logger = logger;
            _smsService = aliyunSmsService;
        }

        [AllowAnonymous]
        [HttpGet(CommonApiConditions.ByMobile)]
        [ProducesResponseType(200)]
        //TODO: Remove this later
        //#if !DEBUG
        //TODO: 将CapthaFilter 抽象出来，不能只依赖腾讯一家
        //[ServiceFilter(typeof(TCapthcaCheckFilter))]
        //#endif
        public async Task<IActionResult> GetByMobileAsync([FromQuery][Mobile] string mobile)
        {
            //_smsService.SendValidationCodeAsync(mobile)
            //    .SafeFireAndForget(ex =>
            //    {
            //        _logger.LogCritical(ex, "短信服务发生故障，请尽快检查!");
            //    });

            //TODO: 调查这个

            //TODO: 同一用户的罚时操作？比如同一手机号，连续请求5次以上？

            await _smsService.SendValidationCodeAsync(mobile).ConfigureAwait(false);

            return Ok(new SmsValidationCodeRes());
        }
    }
}
