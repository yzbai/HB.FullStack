using HB.FullStack.Mobile.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Mobile
{
    //public enum ClientState
    //{
    //    NotLogined,
    //    NewVersionAndLogined,
    //    OldVersionAndLogined
    //}

    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public static class UserPreferences
    {
        private static long? _userId;
        private static string? _mobile;
        private static string? _loginName;
        private static string? _email;
        private static DateTimeOffset? _userCreateTime;
        private static string? _accessToken;
        private static string? _refreshToken;

        public static async Task<long> GetUserIdAsync()
        {
            if (!_userId.HasValue)
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_userId)).ConfigureAwait(false);

                _userId = storedValue == null ? -1 : Convert.ToInt64(storedValue, CultureInfo.InvariantCulture);
            }

            return _userId.Value;
        }

        public static async Task SetUserIdAsync(long userId)
        {
            _userId = userId;

            await PreferenceHelper.PreferenceSetAsync(nameof(_userId), _userId.Value.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
        }

        public static async Task<DateTimeOffset> GetUserCreateTimeAsync()
        {
            if (!_userCreateTime.HasValue)
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_userCreateTime)).ConfigureAwait(false);

                _userCreateTime = storedValue == null ? default : DateTimeOffset.Parse(storedValue, CultureInfo.InvariantCulture);
            }

            return _userCreateTime.Value;
        }

        public static async Task SetUserCreateTimeAsync(DateTimeOffset userCreateTime)
        {
            _userCreateTime = userCreateTime;

            await PreferenceHelper.PreferenceSetAsync(nameof(_userCreateTime), _userCreateTime.Value.ToString(CultureInfo.InvariantCulture)).ConfigureAwait(false);
        }

        public static async Task<string?> GetMobileAsync()
        {
            if (string.IsNullOrEmpty(_mobile))
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_mobile)).ConfigureAwait(false);

                _mobile = storedValue;
            }

            return _mobile;
        }

        public static async Task SetMobileAsync(string? mobile)
        {
            _mobile = mobile;

            await PreferenceHelper.PreferenceSetAsync(nameof(_mobile), mobile).ConfigureAwait(false);
        }

        public static async Task<string?> GetEmailAsync()
        {
            if (string.IsNullOrEmpty(_email))
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_email)).ConfigureAwait(false);

                _email = storedValue;
            }

            return _email;
        }

        public static async Task SetEmailAsync(string? email)
        {
            _email = email;

            await PreferenceHelper.PreferenceSetAsync(nameof(_email), _email).ConfigureAwait(false);
        }

        public static async Task<string?> GetLoginNameAsync()
        {
            if (string.IsNullOrEmpty(_loginName))
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_loginName)).ConfigureAwait(false);

                _loginName = storedValue;
            }

            return _loginName;
        }

        public static async Task SetLoginNameAsync(string? loginName)
        {
            _loginName = loginName;

            await PreferenceHelper.PreferenceSetAsync(nameof(_loginName), _loginName).ConfigureAwait(false);
        }

        public static async Task<string?> GetAccessTokenAsync()
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_accessToken)).ConfigureAwait(false);

                _accessToken = storedValue;
            }

            return _accessToken;
        }

        public static async Task SetAccessTokenAsync(string? accessToken)
        {
            _accessToken = accessToken;

            await PreferenceHelper.PreferenceSetAsync(nameof(_accessToken), _accessToken).ConfigureAwait(false);
        }

        public static async Task<string?> GetRefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(_refreshToken))
            {
                string? storedValue = await PreferenceHelper.PreferenceGetAsync(nameof(_refreshToken)).ConfigureAwait(false);

                _refreshToken = storedValue;
            }

            return _refreshToken;
        }

        public static async Task SetRefreshTokenAsync(string? refreshToken)
        {
            _refreshToken = refreshToken;

            await PreferenceHelper.PreferenceSetAsync(nameof(_refreshToken), _refreshToken).ConfigureAwait(false);
        }

        public static bool IsLogined()
        {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
#pragma warning disable VSTHRD104 // Offer async methods
            string? accessToken = GetAccessTokenAsync().Result;
#pragma warning restore VSTHRD104 // Offer async methods
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

            return !string.IsNullOrEmpty(accessToken);
        }


    }
}
