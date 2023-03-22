using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Identity;
using HB.FullStack.WebApi;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Todo.Server.Main.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class UserTokenController : BaseModelController<UserToken>
    {
        private readonly IIdentityService _identityService;

        public UserTokenController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(
            [Required][LoginName] string loginName, 
            [Required][Password]string password)
        {

        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Register()
        {

        }
    }
}
