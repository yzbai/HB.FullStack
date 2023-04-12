using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Web.Controllers
{
    [Route(".well-known/openid-configuration")]
    [ApiController]
    public class OpenIdConfigurationController : BaseController
    {
        private readonly IdentityOptions _options;
        private readonly IIdentityService _identityService;
        private readonly string _openIdConnectConfigurationString;
        private readonly string _jsonWebKeySet;

        public OpenIdConfigurationController(IOptions<IdentityOptions> options, IIdentityService identityService)
        {
            _options = options.ThrowIfNull(nameof(options)).Value;
            _identityService = identityService.ThrowIfNull(nameof(identityService));

            _openIdConnectConfigurationString = Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration.Write(_options.JwtSettings.OpenIdConnectConfiguration);

            _jsonWebKeySet = _identityService.JsonWebKeySet;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult OpenIdConnectConfiguration()
        {
            return Content(_openIdConnectConfigurationString, "application/json");
        }

        [AllowAnonymous]
        [HttpGet("Jwks")]
        public IActionResult Jwks()
        {
            return Content(_jsonWebKeySet, "application/json");
        }
    }
}
