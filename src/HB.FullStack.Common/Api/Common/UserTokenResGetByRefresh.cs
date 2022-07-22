using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class UserTokenResGetByRefresh : GetRequest<UserTokenRes>
    {
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
        public UserTokenResGetByRefresh() { }

        public UserTokenResGetByRefresh(
            Guid userId,
            string accessToken,
            string refreshToken,
            string deviceId,
            string deviceVersion,
            DeviceInfos deviceInfos)
            : base(nameof(UserTokenRes), ApiRequestAuth.NONE, "ByRefresh")
        {
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceInfos = deviceInfos;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), AccessToken, RefreshToken, UserId, DeviceId, DeviceVersion, DeviceInfos);
        }

        //protected override HttpRequestBuilder CreateHttpRequestBuilder()
        //{
        //    return new RestfulHttpRequestBuilder(ApiMethodName, Auth, Condition, _jwtEndpointSetting.EndpointName, _jwtEndpointSetting.Version, _jwtEndpointSetting.ControllerModelName);
        //}
    }
}