using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.Api
{
    public class UserTokenResGetByRefresh : ApiRequest
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