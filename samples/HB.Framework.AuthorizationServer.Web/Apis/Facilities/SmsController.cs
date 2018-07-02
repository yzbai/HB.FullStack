using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.ComponentModel.DataAnnotations;
using HB.Framework.Services.Sms;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HB.Framework.AuthorizationServer.Web.Apis
{
    public class GetValidationCodeDTO
    {
        [Mobile]
        public string Mobile { get; set; }

        [Required]
        public string ClientId { get; set; }
    }

    [Route("api/[controller]")]
    public class SmsController : Controller
    {
        private ISmsBiz _smsBiz;

        public SmsController(ISmsBiz smsBiz)
        {
            _smsBiz = smsBiz;
        }

        [HttpGet]
        [AllowAnonymous]
        [RequireEntityValidation]
        [RequireImageCodeValidation]
        public async Task<IActionResult> GetIdentityValidationCodeAsync(GetValidationCodeDTO dto)
        {
            SmsResponseResult result = await _smsBiz.SendIdentityValidationCodeAsync(dto.Mobile);
            return Ok();
        }
    }
}
