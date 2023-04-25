using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Identity;
using HB.FullStack.Server.Identity.Context;
using HB.FullStack.Server.Identity.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HB.FullStack.Server.WebLib.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class UserController : BaseModelController<User>
    {
        private readonly ILogger<UserController> _logger;
        private readonly IIdentityService _identityService;

        public UserController(ILogger<UserController> logger, IIdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost(SharedNames.Conditions.ByLoginName)]
        public async Task<IActionResult> RegisterByLoginName([LoginName(CanBeNull = false)] string loginName,
            [Password(CanBeNull = false)] string password,
            [Required] string audience,
            [ValidatedObject][FromQuery] DeviceInfos deviceInfos,
            [Required][FromHeader] string clientId,
            [Required][FromHeader] string clientVersion)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };
            RegisterByLoginName context = new RegisterByLoginName(loginName, password, audience, clientInfos, deviceInfos);

            await _identityService.RegisterAsync(context, "");

            return Ok();
        }
    }
}
