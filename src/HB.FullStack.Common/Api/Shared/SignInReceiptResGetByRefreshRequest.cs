using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.Api
{
    public class SignInReceiptResGetByRefreshRequest : ApiRequest
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
        public string ClientId { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string ClientVersion { get; set; } = null!;

        [RequestQuery]
        [Required]
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public SignInReceiptResGetByRefreshRequest(
            Guid userId,
            string accessToken,
            string refreshToken,
            string clientId,
            string clientVersion,
            DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Get, ApiRequestAuth.NONE, "ByRefresh")
        {
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ClientId = clientId;
            ClientVersion = clientVersion;
            DeviceInfos = deviceInfos;
        }
    }
}