﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared.SignInReceipt;
using HB.FullStack.Identity;
using HB.FullStack.Identity.Context;
using HB.FullStack.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Todo.Server.Main.Controllers
{
    [ApiController]
    [Route($"api/[controller]")]
    public class SignInReceiptController : BaseModelController<SignInReceipt>
    {
        private readonly IIdentityService _identityService;

        public SignInReceiptController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpGet(nameof(LoginByLoginName))]
        public async Task<IActionResult> LoginByLoginName(
            [LoginName(CanBeNull = false)] string loginName,
            [Password(CanBeNull = false)] string password,
            [Required] string audience,
            [Required] string clientId,
            [Required] string clientVersion,
            [ValidatedObject][FromQuery] DeviceInfos deviceInfos)
        {
            ClientInfos clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };

            SignInByLoginName context = new SignInByLoginName(loginName, password, audience, true, SignInExclusivity.None, clientInfos, deviceInfos);

            SignInReceipt receipt = await _identityService.SignInAsync(context, "");

            return Ok(ToRes(receipt));
        }

        [AllowAnonymous]
        [HttpPost(nameof(RegisterByLoginName))]
        public async Task<IActionResult> RegisterByLoginName([LoginName(CanBeNull = false)] string loginName,
            [Password(CanBeNull = false)] string password,
            [Required] string audience,
            [Required] [FromHeader]string clientId,
            [Required] [FromHeader]string clientVersion,
            [ValidatedObject][FromQuery] DeviceInfos deviceInfos)
        {
            ClientInfos         clientInfos = new ClientInfos { ClientId = clientId, ClientVersion = clientVersion, ClientIp = HttpContext.GetIpAddress() };
            RegisterByLoginName context     = new RegisterByLoginName(loginName, password, audience, clientInfos, deviceInfos);

            await _identityService.RegisterAsync(context, "");

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet(nameof(ExceptionTest))]
        [HttpPost(nameof(ExceptionTest))]
        public IActionResult ExceptionTest()
        {
            throw new NotImplementedException();
        }

        private static SignInReceiptRes ToRes(SignInReceipt obj)
        {
            return new SignInReceiptRes
            {
                UserId           = obj.UserId,
                Mobile           = obj.Mobile,
                LoginName        = obj.LoginName,
                Email            = obj.Email,
                EmailConfirmed   = obj.EmailConfirmed,
                MobileConfirmed  = obj.MobileConfirmed,
                TwoFactorEnabled = obj.TwoFactorEnabled,
                CreatedTime      = obj.CreatedTime,
                AccessToken      = obj.AccessToken,
                RefreshToken     = obj.RefreshToken
            };
        }
    }
}
