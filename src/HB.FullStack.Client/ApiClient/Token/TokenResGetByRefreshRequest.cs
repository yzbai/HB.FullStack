/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Shared;



namespace HB.FullStack.Client.ApiClient
{
    internal class TokenResGetByRefreshRequest : ApiRequest
    {
        [RequestQuery]
        [Required]
        public string AccessToken { get; set; } = null!;

        [RequestQuery]
        [Required]
        public string RefreshToken { get; set; } = null!;

        public TokenResGetByRefreshRequest(
            string accessToken,
            string refreshToken)
            : base(nameof(TokenRes), ApiMethod.Get, ApiRequestAuth.NONE, "ByRefresh")
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}