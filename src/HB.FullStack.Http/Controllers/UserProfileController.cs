using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Resources;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Models;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class UserProfileController : BaseController
    {
        private readonly IIdentityService _identityService;

        public UserProfileController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpGet(SharedNames.Conditions.ByUserId)]
        public async Task<IActionResult> GetUserProfileByUserId([FromQuery][NoEmptyGuid] Guid userId)
        {
            //TODO: 权限问题
            UserProfile userProfile = await _identityService.GetUserProfileByUserIdAsync(userId, User.GetLastUser()).ConfigureAwait(false);

            return Ok(ToRes(userProfile));
        }

        [HttpPut]
        public async Task<IActionResult> UpdateUserProfile([FromBody][Required]PropertyChangePack cp)
        {
            await _identityService.UpdateUserProfileAsync(cp, User.GetLastUser()).ConfigureAwait(false);

            return Ok();
        }

        public static UserProfileRes ToRes(UserProfile obj)
        {
            return new UserProfileRes
            {
                Id = obj.Id,
                UserId = obj.UserId,
                Level = obj.Level,
                NickName = obj.NickName,
                Gender = obj.Gender,
                BirthDay = obj.BirthDay,
                AvatarFileName = obj.AvatarFileName
            };
        }
    }
}
