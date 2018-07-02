using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using HB.Framework.Http;
using HB.Framework.AuthorizationServer.Abstractions;
using HB.Framework.Identity.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using HB.Framework.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HB.Framework.AuthorizationServer.Web.Apis
{
    public class LoginBySmsDTO
    {
        [Required]
        [Mobile]
        public string Mobile { get; set; }
        public string SignToWhere { get; set; }

        [Required]
        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
        
    }

    public class RefreshTokenDTO
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [Required]
        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string ClientAddress { get; set; }
    }

    [Route("api/[controller]")]
    public class TokenController : Controller
    {
        private SiteOptions _siteOptions;
        private ISignInManager _signInManager;
        private IRefreshManager _refreshManager;

        public TokenController(IOptions<SiteOptions> options, ISignInManager signInBiz, IRefreshManager refreshManager)
        {
            _siteOptions = options.Value;
            _signInManager = signInBiz;
            _refreshManager = refreshManager;
        }

        [HttpPost]
        [AllowAnonymous]
        [RequireEntityValidation]
        [RequireSmsIdentityValidationCode]
        public async Task<IActionResult> SignInBySmsAsync(LoginBySmsDTO dto)
        {
            ClientType clientType = ClientType.None;
            Enum.TryParse(dto.ClientType, out clientType);

            Abstractions.SignInResult result = await _signInManager.SignInAsync(new SignInContext() {
                SignInType = SignInType.BySms,
                Mobile = dto.Mobile,
                RememberMe = true,
                ClientId = dto.ClientId,
                ClientType = clientType,
                ClientVersion = dto.ClientVersion,
                ClientAddress = dto.ClientAddress,
                ClientIp = HttpContext.GetIpAddress(),
                SignToWhere = dto.SignToWhere
            });

            if (result == Abstractions.SignInResult.Success)
            {
                return Json(new { AccessToken = result.AccessToken, RfreshToken = result.RefreshToken });
            }

            return BadRequest(new { Message = result.Status.GetDescription() });
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> SignOutAsync()
        {
            await _signInManager.SignOutAsync(new SignOutContext() { HttpContext = this.HttpContext });

            return Ok();
        }

        
        [HttpPut]
        [AllowAnonymous]
        [RequireEntityValidation]
        public async Task<IActionResult> RefreshAsync(RefreshTokenDTO dto)
        {
            ClientType clientType = ClientType.None;
            Enum.TryParse(dto.ClientType, out clientType);

            RefreshResult result = await _refreshManager.RefreshAccessTokenAsync(new RefreshContext() {
                AccessToken = dto.AccessToken,
                RefreshToken = dto.RefreshToken,
                ClientId = dto.ClientId,
                ClientAddress = dto.ClientAddress,
                ClientType = clientType,
                ClientVersion = dto.ClientVersion
            });

            if (result.Succeed)
            {
                return Json(new { AccessToken = result.AccessToken, RfreshToken = result.RefreshToken });
            }

            return BadRequest(new { AccessToken = result.AccessToken, RfreshToken = result.RefreshToken });
        }
    }
}
