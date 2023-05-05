using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.WebLib.Services;

using Microsoft.AspNetCore.Mvc;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class DirectoryTokenController : BaseController
    {
        private readonly ILogger<DirectoryTokenController> _logger;
        private readonly IDirectoryTokenService _directoryTokenService;

        public DirectoryTokenController(ILogger<DirectoryTokenController> logger, IDirectoryTokenService directoryTokenService)
        {
            _logger = logger;
            _directoryTokenService = directoryTokenService;
        }

        //Rule: 确保LastUser被正确获取
        [HttpGet(SharedNames.Conditions.ByDirectoryPermissionName)]
        [ProducesResponseType(typeof(DirectoryTokenRes), 200)]
        public async Task<IActionResult> GetByDirectoryPermissionNameAsync(
            [Required] string directoryPermissionName,
                       string? regexPlaceHolderValue,
            [Required] bool readOnly)
        {
            DirectoryToken? stsTokenRes = await _directoryTokenService.GetDirectoryTokenAsync(
                User.GetUserId().GetValueOrDefault(),
                directoryPermissionName,
                regexPlaceHolderValue,
                readOnly,
                User.GetLastUser()).ConfigureAwait(false);

            if (stsTokenRes == null)
            {
                return Error(ErrorCodes.DirectoryTokenNotFound);
            }

            return Ok(ToRes(stsTokenRes));
        }

        //TODO: 需要不需要对匿名开放StsToken 获取？ public 图片怎么获取？

        private static DirectoryTokenRes ToRes(DirectoryToken obj)
        {
            return new DirectoryTokenRes
            {
                UserId = obj.UserId,
                SecurityToken = obj.SecurityToken,
                AccessKeyId = obj.AccessKeyId,
                AccessKeySecret = obj.AccessKeySecret,
                ExpirationAt = obj.ExpirationAt,
                DirectoryPermissionName = obj.DirectoryPermissionName,
                ReadOnly = obj.ReadOnly
            };
        }
    }
}
