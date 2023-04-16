using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;

using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Client.ApiClient
{
    internal class SignInReceiptResGetByRefreshRequest : ApiRequest
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
        public DeviceInfos DeviceInfos { get; set; } = null!;

        public SignInReceiptResGetByRefreshRequest(
            Guid userId,
            string accessToken,
            string refreshToken,
            DeviceInfos deviceInfos)
            : base(nameof(SignInReceiptRes), ApiMethod.Get, ApiRequestAuth.NONE, "ByRefresh")
        {
            UserId = userId;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            DeviceInfos = deviceInfos;
        }
    }
}