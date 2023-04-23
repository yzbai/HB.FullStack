/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using HB.FullStack.Common.Shared;

using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Server.Identity
{
    public class RefreshContext : ValidatableObject
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }

        [ValidatedObject(CanBeNull = false)]
        public ClientInfos ClientInfos { get; set; }

        public RefreshContext(string accessToken, string refreshToken, ClientInfos clientInfos)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            ClientInfos = clientInfos;
        }
    }
}