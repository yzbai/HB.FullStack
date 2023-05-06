/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class TokenController : BaseModelController<Token>
    {
        private readonly ILogger<TokenController> _logger;
        private readonly ISmsService _smsService;
        private readonly IIdentityService _identityService;

        public TokenController(ILogger<TokenController> logger, ISmsService smsService, IIdentityService identityService)
        {
            _logger = logger;
            _smsService = smsService;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpGet(SharedNames.Conditions.BySms)]
        [ProducesResponseType(typeof(TokenRes), 200)]
        public async Task<IActionResult> GetBySmsAsync(
            [FromQuery][Mobile(CanBeNull = false)] string mobile,
            [FromQuery][SmsCode(CanBeNull = false)] string smsCode,
            [FromQuery][Required] string audience,
            [FromQuery][Required] DeviceInfos deviceInfos,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion)
        {
            string lastUser = Conventions.GetLastUser(null, mobile, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            SignInContext context = new SignInBySms(mobile, smsCode, true, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            Token token = await _identityService.GetTokenAsync(context, lastUser).ConfigureAwait(false);

            TokenRes res = ToRes(token);

            return Ok(res);
        }

        //TODO: 考虑 Refresh Token Rotaion
        //https://auth0.com/docs/tokens/access-tokens/refresh-tokens/refresh-token-rotation/use-refresh-token-rotation
        /// <summary>
        /// 可能同时收到一大批刷新请求，只在一定时间间隔里相应一个
        /// </summary>
        [AllowAnonymous]
        [HttpGet(SharedNames.Conditions.ByRefresh)]
        [ProducesResponseType(typeof(TokenRes), 200)]
        public async Task<IActionResult> GetByRefreshAsync(
            [FromQuery][Required] string accessToken,
            [FromQuery][Required] string refreshToken,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion)
        {
            //TODO:其他安全检测
            //1， 频率. ClientId, IP. 根据频率来决定客户端要不要弹出防水墙
            //2， 历史登录比较 Mobile和ClientId绑定。Address

            //检查从某IP，ClientId，Mobile发来的请求是否需要防水墙。
            //需要的话，查看request.PublicResourceToken. 没有的话，返回ErrorCode.API_NEED_PUBLIC_RESOURCE_TOKEN

            //某一个accesstoken失败，直接拉黑

            string lastUser = Conventions.GetLastUser(null, null, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            RefreshContext context = new RefreshContext(accessToken, refreshToken, clientInfos);

            Token token = await _identityService.RefreshTokenAsync(context, lastUser).ConfigureAwait(false);

            TokenRes res = ToRes(token);

            return Ok(res);
        }

        /// <summary>
        /// 删除了RefreshToken，但已经颁发出去的未过期的AccessToken依然有效，所以AccessToken的有效期要足够小。
        /// 或者设置黑名单，但这样就会带来查询负担。纯jwt验证其实是用cpu计算时间代替了查询时间
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteAsync()
        {
            await _identityService.DeleteTokenAsync(User.GetTokenCredentialId().GetValueOrDefault(), User.GetLastUser()!).ConfigureAwait(false);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet(SharedNames.Conditions.ByLoginName)]
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

            Token receipt = await _identityService.GetTokenAsync(context, "");

            return Ok(ToRes(receipt));
        }

        private static TokenRes ToRes(Token obj)
        {
            return new TokenRes
            {
                UserId = obj.UserId,
                UserLevel = obj.UserLevel,
                Mobile = obj.Mobile,
                LoginName = obj.LoginName,
                Email = obj.Email,
                EmailConfirmed = obj.EmailConfirmed,
                MobileConfirmed = obj.MobileConfirmed,
                TwoFactorEnabled = obj.TwoFactorEnabled,
                TokenCreatedTime = obj.TokenCreatedTime,
                AccessToken = obj.AccessToken,
                RefreshToken = obj.RefreshToken
            };
        }
    }
}