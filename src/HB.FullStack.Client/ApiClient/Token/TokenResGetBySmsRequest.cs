﻿using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class TokenResGetBySmsRequest : ApiRequest
    {
        [RequestQuery]
        [Mobile(CanBeNull = false)]
        public string Mobile { get; set; } = null!;

        [RequestQuery]
        [SmsCode(CanBeNull = false)]
        public string SmsCode { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string Audience { get; set; } = null!;

        [RequestQuery]
        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public TokenResGetBySmsRequest(
            string mobile, string smsCode, string signToWhere, DeviceInfos deviceInfos)
            : base(nameof(TokenRes), ApiMethod.Get, ApiRequestAuth.NONE, SharedNames.Conditions.BySms)
        {
            Mobile = mobile;
            SmsCode = smsCode;
            Audience = signToWhere;
            DeviceInfos = deviceInfos;
        }
    }
}
