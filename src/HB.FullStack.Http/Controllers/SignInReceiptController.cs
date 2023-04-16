using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using HB.FullStack.Common.Server;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;
using HB.FullStack.Identity;
using HB.FullStack.Identity.Context;
using HB.FullStack.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Todo.Server.Main.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class SignInReceiptController : BaseModelController<SignInReceipt>
    {
        private readonly ILogger<SignInReceiptController> _logger;
        private readonly ISmsServerService _smsService;
        private readonly IIdentityService _identityService;

        public SignInReceiptController(ILogger<SignInReceiptController> logger, ISmsServerService smsService, IIdentityService identityService)
        {
            _logger = logger;
            _smsService = smsService;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpGet(CommonApiConditions.BySms)]
        [ProducesResponseType(typeof(SignInReceiptRes), 200)]
        public async Task<IActionResult> GetBySmsAsync(
            [FromQuery][Mobile(CanBeNull  = false)] string      mobile,
            [FromQuery][SmsCode(CanBeNull = false)] string      smsCode,
            [FromQuery][Required]                   string      audience,
            [FromQuery][Required]                   DeviceInfos deviceInfos,
            [FromHeader][Required]                  string      clientId,
            [FromHeader][Required]                  string      clientVersion)
        {
            string lastUser = CommonConventions.GetLastUser(null, mobile, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            SignInContext context = new SignInBySms(mobile, smsCode, true, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            SignInReceipt signInReceipt = await _identityService.SignInAsync(context, lastUser).ConfigureAwait(false);

            SignInReceiptRes res = ToRes(signInReceipt);

            return Ok(res);
        }

        //TODO: 考虑 Refresh Token Rotaion
        //https://auth0.com/docs/tokens/access-tokens/refresh-tokens/refresh-token-rotation/use-refresh-token-rotation
        /// <summary>
        /// 可能同时收到一大批刷新请求，只在一定时间间隔里相应一个
        /// </summary>
        [AllowAnonymous]
        [HttpGet(CommonApiConditions.ByRefresh)]
        [ProducesResponseType(typeof(SignInReceiptRes), 200)]
        public async Task<IActionResult> GetByRefreshAsync(
            [FromQuery][NoEmptyGuid] Guid userId,
            [FromQuery][Required] string accessToken,
            [FromQuery][Required] string refreshToken,
            [FromQuery][Required] DeviceInfos deviceInfos,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion)
        {
            //TODO:其他安全检测
            //1， 频率. ClientId, IP. 根据频率来决定客户端要不要弹出防水墙
            //2， 历史登录比较 Mobile和ClientId绑定。Address

            //检查从某IP，ClientId，Mobile发来的请求是否需要防水墙。
            //需要的话，查看request.PublicResourceToken. 没有的话，返回ErrorCode.API_NEED_PUBLIC_RESOURCE_TOKEN

            //某一个accesstoken失败，直接拉黑

            string lastUser = CommonConventions.GetLastUser(userId, null, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            RefreshContext context = new RefreshContext(accessToken, refreshToken, clientInfos, deviceInfos);

            SignInReceipt signInReceipt = await _identityService.RefreshSignInReceiptAsync(context, lastUser).ConfigureAwait(false);

            SignInReceiptRes res = ToRes(signInReceipt);

            return Ok(res);
        }

        /// <summary>
        /// 删除了RefreshToken，但已经颁发出去的未过期的AccessToken依然有效，所以AccessToken的有效期要足够小。
        /// 或者设置黑名单，但这样就会带来查询负担。纯jwt验证其实是用cpu计算时间代替了查询时间
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync()
        {
            await _identityService.SignOutAsync(User.GetSignInCredentialId().GetValueOrDefault(), User.GetLastUser()!).ConfigureAwait(false);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet(CommonApiConditions.ByLoginName)]
        public async Task<IActionResult> GetByLoginName(
            [LoginName(CanBeNull = false)] string loginName,
            [Password(CanBeNull = false)] string password,
            [Required] string audience,
            [Required][FromHeader] string clientId,
            [Required][FromHeader] string clientVersion,
            [ValidatedObject(CanBeNull = false)][FromQuery] DeviceInfos deviceInfos)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            SignInByLoginName context = new SignInByLoginName(loginName, password, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            SignInReceipt receipt = await _identityService.SignInAsync(context, "");

            return Ok(ToRes(receipt));
        }

        [AllowAnonymous]
        [HttpPost(CommonApiConditions.ByLoginName)]
        public async Task<IActionResult> RegisterByLoginName([LoginName(CanBeNull = false)] string loginName,
            [Password(CanBeNull = false)] string password,
            [Required] string audience,
            [Required][FromHeader] string clientId,
            [Required][FromHeader] string clientVersion,
            [ValidatedObject][FromQuery] DeviceInfos deviceInfos)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };
            RegisterByLoginName context = new RegisterByLoginName(loginName, password, audience, clientInfos, deviceInfos);

            await _identityService.RegisterAsync(context, "");

            return Ok();
        }


        private static SignInReceiptRes ToRes(SignInReceipt obj)
        {
            return new SignInReceiptRes
            {
                UserId = obj.UserId,
                Mobile = obj.Mobile,
                LoginName = obj.LoginName,
                Email = obj.Email,
                EmailConfirmed = obj.EmailConfirmed,
                MobileConfirmed = obj.MobileConfirmed,
                TwoFactorEnabled = obj.TwoFactorEnabled,
                CreatedTime = obj.CreatedTime,
                AccessToken = obj.AccessToken,
                RefreshToken = obj.RefreshToken
            };
        }
    }
}
