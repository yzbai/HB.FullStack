using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class BearerTokenResGetBySmsRequest : ApiRequest
    {
        private readonly JwtEndpointSetting _jwtEndpointSetting = null!;

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

        public BearerTokenResGetBySmsRequest(JwtEndpointSetting jwtEndpointSetting, string mobile, string smsCode, string signToWhere, string deviceId, string deviceVersion, DeviceInfos deviceInfos)
            : base(ApiMethodName.Get, ApiRequestAuth.NONE, "BySms")
        {
            _jwtEndpointSetting = jwtEndpointSetting;

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

        protected override HttpRequestBuilder CreateHttpRequestBuilder()
        {
            return new RestfulHttpRequestBuilder(ApiMethodName, Auth, Condition, _jwtEndpointSetting.EndpointName, _jwtEndpointSetting.Version, _jwtEndpointSetting.ResName);
        }
    }
}
