using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class UserProfileApi
    {
        public static RouteGroupBuilder MapUserProfileApi<TId>(this RouteGroupBuilder group)
        {
            group.MapGet(SharedNames.Conditions.ByUserId, GetUserProfileByUserId<TId>);
            group.MapPatch("/", UpdateUserProfile<TId>);

            return group;
        }

        private static async Task<IResult> UpdateUserProfile<TId>([FromBody][Required] PropertyChangePack cp, [FromServices] IIdentityService<TId> identityService, ClaimsPrincipal User)
        {
            await identityService.UpdateUserProfileAsync(cp, User.GetLastUser()).ConfigureAwait(false);

            return Results.Ok();
        }

        private static async Task<IResult> GetUserProfileByUserId<TId>([FromQuery][NoEmptyGuid] TId userId, [FromServices] IIdentityService<TId> identityService, ClaimsPrincipal User)
        {
            //TODO: 权限问题
            UserProfile<TId> userProfile = await identityService.GetUserProfileByUserIdAsync(userId, User.GetLastUser()).ConfigureAwait(false);

            return TypedResults.Ok(ToRes(userProfile));
        }

        public static UserProfileRes<TId> ToRes<TId>(UserProfile<TId> obj)
        {
            return new UserProfileRes<TId>
            {
                Id = obj.Id,
                ExpiredAt = ?,//TODO: 统一配置

                UserId = obj.UserId,
                NickName = obj.NickName,
                Gender = obj.Gender,
                BirthDay = obj.BirthDay,
                AvatarFileName = obj.AvatarFileName
            };
        }
    }
}
