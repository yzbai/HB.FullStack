using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Common.Api
{
    public class UserTokenResGetBySms : ApiRequest
    {
        [Required]
        [Mobile]
        public string Mobile { get; set; } = null!;

        [Required]
        [SmsCode]
        public string SmsCode { get; set; } = null!;

        [Required]
        public string SignToWhere { get; set; } = null!;

        [Required]
        public string DeviceId { get; set; } = null!;

        [Required]
        public string DeviceVersion { get; set; } = null!;

        public DeviceInfos DeviceInfos { get; set; } = null!;

        public UserTokenResGetBySms() { }

        public UserTokenResGetBySms(string mobile, string smsCode, string signToWhere, string deviceId, string deviceVersion, DeviceInfos deviceInfos)
            : base(nameof(UserTokenRes), ApiMethodName.Get, ApiRequestAuth.NONE, "BySms")
        {
            Mobile = mobile;
            SmsCode = smsCode;
            SignToWhere = signToWhere;
            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceInfos = deviceInfos;
        }
    }
}
