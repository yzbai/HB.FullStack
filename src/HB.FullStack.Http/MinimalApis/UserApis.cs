using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Context;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class UserApis
    {
        public static RouteGroupBuilder MapUserApis<TId>(this RouteGroupBuilder builder)
        {
            builder.MapPost(SharedNames.Conditions.ByLoginName, RegisterByLoginName<TId>).AllowAnonymous();
            return builder;
        }

        public static async Task<IResult> RegisterByLoginName<TId>(
            [FromQuery][LoginName(CanBeNull = false)] string loginName,
            [FromQuery][Password(CanBeNull = false)] string password,
            [FromQuery][Required] string audience,
            [FromQuery][ValidatedObject] DeviceInfos deviceInfos,
            [FromHeader][Required] string clientId,
            [FromHeader][Required] string clientVersion,
            [FromServices] IIdentityService<TId> identityService,
            HttpContext httpContext)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = httpContext.GetIpAddress() };
            RegisterByLoginName context = new RegisterByLoginName(loginName, password, audience, clientInfos, deviceInfos);

            await identityService.RegisterUserAsync(context, "");

            return Results.Ok();
        }
    }
}
