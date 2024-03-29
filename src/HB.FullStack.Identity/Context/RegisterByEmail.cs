﻿using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Server.Identity.Context
{
    public class RegisterByEmail : RegisterContext
    {
        public RegisterByEmail(string email, string emailCode, string audience, ClientInfos clientInfos, DeviceInfos deviceInfos) : base(audience, clientInfos, deviceInfos)
        {
            Email = email;
            EmailCode = emailCode;
        }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string EmailCode { get; set; }

    }
}
