/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common.Files;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Client
{
    public class ClientOptions : IOptions<ClientOptions>
    {
        public ClientOptions Value => this;

        #region Expiry Time Settings

        public int TinyExpirySeconds { get; set; } = 10;

        public int ShortExpirySeconds { get; set; } = 60;

        public int MediumExpirySeconds { get; set; } = 5 * 60;

        public int LongExpirySeconds { get; set; } = 60 * 60;

        #endregion

        #region User Settings

        public Directory2 AvatarDirectory { get; set; } = null!;

        public bool NeedLoginDefault { get; set; } = true;

        #endregion
    }
}