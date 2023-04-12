using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Server;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Sts;
using HB.Infrastructure.Aliyun.Sts;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Web.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class StsTokenController : BaseController
    {
        private readonly ILogger<StsTokenController> _logger;
        private readonly IStsTokenService? _stsTokenService;

        public StsTokenController(ILogger<StsTokenController> logger, IStsTokenService? stsTokenService)
        {
            _logger = logger;
            _stsTokenService = stsTokenService;
        }

        //Rule: 确保LastUser被正确获取
        [HttpGet(CommonApiConditions.ByDirectoryPermissionName)]
        [ProducesResponseType(typeof(StsTokenRes), 200)]
        public async Task<IActionResult> GetByDirectoryPermissionNameAsync(
            [Required] string  directoryPermissionName,
                       string? regexPlaceHolderValue,
            [Required] bool    readOnly)
        {
            _stsTokenService.ThrowIfNull(nameof(_stsTokenService));

            StsTokenRes? stsTokenRes = await _stsTokenService.GetAliyunOssStsTokenAsync(
                User.GetUserId().GetValueOrDefault(),
                directoryPermissionName,
                regexPlaceHolderValue,
                readOnly,
                User.GetLastUser()).ConfigureAwait(false);

            return Ok(stsTokenRes);
        }

        //TODO: 需要不需要对匿名开放StsToken 获取？ public 图片怎么获取？
    }
}
