using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.Api
{
    public class UserTokenResGetByRefresh : ApiRequest
    {
        [RequestQuery]
        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [RequestQuery]
        [Required]
        public string AccessToken { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string RefreshToken { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string DeviceId { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string DeviceVersion { get; set; } = null!;

        [RequestQuery]
        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public UserTokenResGetByRefresh(
            Guid userId,
            string accessToken,
            string refreshToken,
            string deviceId,
            string deviceVersion,
            DeviceInfos deviceInfos)
            : base(nameof(UserTokenRes), ApiMethod.Get, ApiRequestAuth2.NONE, "ByRefresh")
        {
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            DeviceId = deviceId;
            DeviceVersion = deviceVersion;
            DeviceInfos = deviceInfos;
        }
    }
}