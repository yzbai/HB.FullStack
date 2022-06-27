using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api.Requests;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class BearerTokenResGetBySmsRequest : GetRequest<BearerTokenRes>
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

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public BearerTokenResGetBySmsRequest() { }

        public BearerTokenResGetBySmsRequest(string mobile, string smsCode, string signToWhere, string deviceId, string deviceVersion, DeviceInfos deviceInfos)
            : base(ApiRequestAuth.NONE, "BySms")
        {
            Mobile = mobile;
            SmsCode = smsCode;
            SignToWhere = signToWhere;
            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceInfos = deviceInfos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Mobile, SmsCode, SignToWhere, DeviceId, DeviceVersion);
        }
    }
}
