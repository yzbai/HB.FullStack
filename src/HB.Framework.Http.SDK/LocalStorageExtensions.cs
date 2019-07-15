using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.Http.SDK
{
    public static class LocalStorageExtensions
    {
        private const string AccessToken = "AccessToken";
        private const string RefreshToken = "RefreshToken";

        public static Task<string> GetAccessTokenAsync(this ILocalStorage localStorage)
        {
            return localStorage.ThrowIfNull(nameof(localStorage)).GetAsync(AccessToken);
        }

        public static Task SetAccessTokenAsync(this ILocalStorage localStorage, string value)
        {
            return localStorage.ThrowIfNull(nameof(localStorage)).SetAsync(AccessToken, value);
        }

        public static Task<string> GetRefreshTokenAsync(this ILocalStorage localStorage)
        {
            return localStorage.ThrowIfNull(nameof(localStorage)).GetAsync(RefreshToken);
        }

        public static Task SetRefreshTokenAsync(this ILocalStorage localStorage, string value)
        {
            return localStorage.ThrowIfNull(nameof(localStorage)).SetAsync(RefreshToken, value);
        }
    }
}
