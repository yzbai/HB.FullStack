using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using HB.Framework.Identity;
using HB.Framework.Identity.Abstractions;
using System.ComponentModel.DataAnnotations;


namespace HB.Framework.AuthorizationServer.Web.Apis
{
    public class RegisterByMobileDTO
    {
        [Required]
        [Mobile]
        public string Mobile { get; set; }

        [Required]
        public string ClientId { get; set; }
    }

    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private IUserBiz _userBiz;

        public UserController(IUserBiz userBiz)
        {
            _userBiz = userBiz;
        }

        [HttpPost]
        [AllowAnonymous]
        [RequireEntityValidation]
        [RequireSmsIdentityValidationCode]
        public async Task<IActionResult> RegisterByMobileAsync(RegisterByMobileDTO dto)
        {
            IdentityResult result = await _userBiz.CreateUserByMobileAsync(dto.Mobile, null, null, true);

            if (result == IdentityResult.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}
