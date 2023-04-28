using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
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
        public async Task<IActionResult> GetByUserId([FromQuery][NoEmptyGuid] Guid userId)
        {
            //TODO: 权限问题
            UserProfile userProfile = await _identityService.GetUserProfileByUserIdAsync(userId, User.GetLastUser()).ConfigureAwait(false);

            return Ok(userProfile);
        }
    }
}
