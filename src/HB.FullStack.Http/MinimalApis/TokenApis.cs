/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class TokenApis
    {
        public static RouteGroupBuilder MapTokenApis<TId>(this RouteGroupBuilder builder)
        {
            builder.MapGet(SharedNames.Conditions.BySms, GetBySms<TId>).AllowAnonymous();
            builder.MapGet(SharedNames.Conditions.ByRefresh, GetByRefresh<TId>).AllowAnonymous();
            builder.MapGet(SharedNames.Conditions.ByLoginName, GetByLoginName<TId>).AllowAnonymous();
            builder.MapDelete("/", DeleteAsync<TId>).RequireAuthorization();

            return builder;
        }

        /// <summary>
        /// 删除了RefreshToken，但已经颁发出去的未过期的AccessToken依然有效，所以AccessToken的有效期要足够小。
        /// 或者设置黑名单，但这样就会带来查询负担。纯jwt验证其实是用cpu计算时间代替了查询时间
        /// </summary>
        private static async Task<IResult> DeleteAsync<TId>([FromServices] IIdentityService<TId> identityService, ClaimsPrincipal User)
        {
            await identityService.DeleteTokenAsync(User.GetTokenCredentialId<TId>(), User.GetLastUser()!).ConfigureAwait(false);

            return Results.Ok();
        }

        private static async Task<IResult> GetByLoginName<TId>(
            [FromQuery][LoginName(CanBeNull = false)] string loginName,
            [FromQuery][Password(CanBeNull = false)] string password,
            [FromQuery][Required] string audience,
            [FromQuery][ValidatedObject(CanBeNull = false)] DeviceInfos deviceInfos,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion,
            [FromServices] IIdentityService<TId> identityService,
            HttpContext httpContext)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = httpContext.GetIpAddress() };

            SignInByLoginName context = new SignInByLoginName(loginName, password, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            Token<TId> receipt = await identityService.GetTokenAsync(context, "");

            return TypedResults.Ok(ToRes(receipt));
        }

        //TODO: 考虑 Refresh Token Rotaion
        //https://auth0.com/docs/tokens/access-tokens/refresh-tokens/refresh-token-rotation/use-refresh-token-rotation
        /// <summary>
        /// 可能同时收到一大批刷新请求，只在一定时间间隔里相应一个
        /// </summary>
        private static async Task<IResult> GetByRefresh<TId>(
            [FromQuery][Required] string accessToken,
            [FromQuery][Required] string refreshToken,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion,
            [FromServices] IIdentityService<TId> identityService,
            HttpContext httpContext)
        {
            //TODO:其他安全检测
            //1， 频率. ClientId, IP. 根据频率来决定客户端要不要弹出防水墙
            //2， 历史登录比较 Mobile和ClientId绑定。Address

            //检查从某IP，ClientId，Mobile发来的请求是否需要防水墙。
            //需要的话，查看request.PublicResourceToken. 没有的话，返回ErrorCode.API_NEED_PUBLIC_RESOURCE_TOKEN

            //某一个accesstoken失败，直接拉黑

            string lastUser = Conventions.GetLastUser(null, null, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = httpContext.GetIpAddress() };

            RefreshContext context = new RefreshContext(accessToken, refreshToken, clientInfos);

            Token<TId> token = await identityService.RefreshTokenAsync(context, lastUser).ConfigureAwait(false);

            return TypedResults.Ok(ToRes(token));
        }

        private static async Task<IResult> GetBySms<TId>(
            [FromQuery][Mobile(CanBeNull = false)] string mobile,
            [FromQuery][SmsCode(CanBeNull = false)] string smsCode,
            [FromQuery][Required] string audience,
            [FromQuery][Required] DeviceInfos deviceInfos,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion,
            [FromServices] IIdentityService<TId> identityService,
            HttpContext httpContext)
        {
            string lastUser = Conventions.GetLastUser(null, mobile, clientId);

            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = httpContext.GetIpAddress() };

            SignInContext context = new SignInBySms(mobile, smsCode, true, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            Token<TId> token = await identityService.GetTokenAsync(context, lastUser).ConfigureAwait(false);

            return TypedResults.Ok(ToRes(token));
        }

        private static TokenRes<TId> ToRes<TId>(Token<TId> obj)
        {
            return new TokenRes<TId>
            {
                Id = default,
                ExpiredAt = obj.ExpiredAt,

                UserId = obj.UserId,
                UserLevel = obj.UserLevel,
                Mobile = obj.Mobile,
                LoginName = obj.LoginName,
                Email = obj.Email,
                EmailConfirmed = obj.EmailConfirmed,
                MobileConfirmed = obj.MobileConfirmed,
                TwoFactorEnabled = obj.TwoFactorEnabled,
                AccessToken = obj.AccessToken,
                RefreshToken = obj.RefreshToken
            };
        }
    }
}