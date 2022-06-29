using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class BearerTokenResGetByRefreshRequest : ApiRequest
    {
        private readonly JwtEndpointSetting _jwtEndpointSetting = null!;

        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [Required]
        public string AccessToken { get; set; } = null!;

        [Required]
        public string RefreshToken { get; set; } = null!;

        [Required]
        public string DeviceId { get; set; } = null!;

        [Required]
        public string DeviceVersion { get; set; } = null!;

        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        /// <summary>
        /// Only for Deserialization
        /// </summary>
        public BearerTokenResGetByRefreshRequest() { }

        public BearerTokenResGetByRefreshRequest(
            JwtEndpointSetting jwtEndpointSetting,
            Guid userId,
            string accessToken,
            string refreshToken,
            string deviceId,
            string deviceVersion,
            DeviceInfos deviceInfos)
            : base(ApiMethodName.Get, ApiRequestAuth.NONE, "ByRefresh")
        {
            _jwtEndpointSetting = jwtEndpointSetting;
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceInfos = deviceInfos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), AccessToken, RefreshToken, UserId, DeviceId, DeviceVersion);
        }

        protected override HttpRequestBuilder CreateHttpRequestBuilder()
        {
            return new RestfulHttpRequestBuilder(ApiMethodName, Auth, Condition, _jwtEndpointSetting.EndpointName, _jwtEndpointSetting.Version, _jwtEndpointSetting.ResName);
        }
    }
}