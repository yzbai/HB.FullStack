﻿using System.ComponentModel.DataAnnotations;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Server.Identity.Context
{
    public class RegisterBySms : RegisterContext, IBySmsCode
    {
        public RegisterBySms(string mobile, string smsCode, string audience, ClientInfos clientInfos, DeviceInfos deviceInfos) : base(audience, clientInfos, deviceInfos)
        {
            Mobile = mobile;
            SmsCode = smsCode;
        }

        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; }

        [SmsCode(CanBeNull = false)]
        public string SmsCode { get; set; }
    }
}
