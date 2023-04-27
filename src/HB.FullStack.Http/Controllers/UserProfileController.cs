using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;

using Microsoft.AspNetCore.Mvc;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class UserProfileController : BaseController
    {
        public UserProfileController() { }

        [HttpGet(SharedNames.Conditions.ByUserId)]
        public async Task<IActionResult> GetByUserId([FromQuery] Guid userId)
        {
            return Ok();
        }
    }
}
