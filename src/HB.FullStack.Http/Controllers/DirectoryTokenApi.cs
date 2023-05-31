using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity.Models;
using HB.FullStack.Server.WebLib.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Controllers
{
    public static class DirectoryTokenApi
    {
        public static RouteGroupBuilder MapDirectoryTokenApi<TId>(this RouteGroupBuilder group)
        {
            group.MapGet(SharedNames.Conditions.ByDirectoryPermissionName, GetByDirectoryPermissionName<TId>);

            return group;
        }

        private static IResult GetByDirectoryPermissionName<TId>(
            [FromQuery][Required] string directoryPermissionName,
            [FromQuery] string? placeHolderValue,
            [FromQuery][Required] bool readOnly, 
            [FromServices]IDirectoryTokenService<TId> directoryTokenService,
            ClaimsPrincipal User)
        {
            DirectoryToken<TId>? directoryToken = directoryTokenService.GetDirectoryToken(
                User.GetUserId<TId>()!,
                User.GetUserLevel(),
                directoryPermissionName,
                placeHolderValue,
                readOnly);

            if (directoryToken == null)
            {
                return Results.BadRequest(ErrorCodes.DirectoryTokenNotFound);
            }

            return Results.Ok(ToRes(directoryToken));
        }

        private static DirectoryTokenRes<TId> ToRes<TId>(DirectoryToken<TId> obj)
        {
            return new DirectoryTokenRes<TId>
            {
                UserId = obj.UserId,
                SecurityToken = obj.SecurityToken,
                AccessKeyId = obj.AccessKeyId,
                AccessKeySecret = obj.AccessKeySecret,
                ExpiredAt = obj.ExpiredAt,
                DirectoryPermissionName = obj.DirectoryPermissionName,
                ReadOnly = obj.ReadOnly
            };
        }
    }
}
