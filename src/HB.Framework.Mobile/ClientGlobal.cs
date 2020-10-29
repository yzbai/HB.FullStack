using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.Framework.Client
{
    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public static class ClientGlobal
    {
        private static string? _deviceId;
        private static string? _deviceType;
        private static string? _deviceVersion;
        private static string? _deviceAddress;

        private static string? _currentUserGuid;
        private static string? _accessToken;
        private static string? _refreshToken;
        private static string? _currentLoginName;
        private static string? _currentMobile;
        private static string? _currentEmail;

        #region Device

        public static async Task<string> GetDeviceIdAsync()
        {
            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId!;
            }

            _deviceId = await PreferenceGetAsync(ClientNames.DeviceId).ConfigureAwait(false);

            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId!;
            }

            _deviceId = ClientUtils.CreateNewDeviceId();

            await PreferenceSetAsync(ClientNames.DeviceId, _deviceId).ConfigureAwait(false);

            return _deviceId!;
        }

        public static async Task<string> GetDeviceTypeAsync()
        {
            if (_deviceType.IsNotNullOrEmpty())
            {
                return _deviceType!;
            }

            _deviceType = await PreferenceGetAsync(ClientNames.DeviceType).ConfigureAwait(false);

            if (_deviceType.IsNotNullOrEmpty())
            {
                return _deviceType!;
            }

            _deviceType = ClientUtils.GetDeviceType();

            await PreferenceSetAsync(ClientNames.DeviceType, _deviceType).ConfigureAwait(false);

            return _deviceType!;
        }

        public static async Task<string> GetDeviceVersionAsync()
        {
            if (_deviceVersion.IsNotNullOrEmpty())
            {
                return _deviceVersion!;
            }

            _deviceVersion = await PreferenceGetAsync(ClientNames.DeviceVersion).ConfigureAwait(false);

            if (_deviceVersion.IsNotNullOrEmpty())
            {
                return _deviceVersion!;
            }

            _deviceVersion = ClientUtils.GetDeviceVersion();

            await PreferenceSetAsync(ClientNames.DeviceVersion, _deviceVersion).ConfigureAwait(false);

            return _deviceVersion!;
        }

        public static async Task<string> GetDeviceAddressAsync()
        {
            //if (_deviceAddress.IsNotNullOrEmpty())
            //{
            //    return _deviceAddress!;
            //}

            //_deviceAddress = await PreferenceGetAsync(ClientNames.DeviceAddress).ConfigureAwait(false);

            //if (_deviceAddress.IsNotNullOrEmpty())
            //{
            //    return _deviceAddress!;
            //}

            _deviceAddress = await ClientUtils.GetDeviceAddressAsync().ConfigureAwait(false);

            //await PreferenceSetAsync(ClientNames.DeviceAddress, _deviceAddress).ConfigureAwait(false);

            return _deviceAddress!;
        }

        #endregion

        #region User

        public static async Task<bool> IsLoginedAsync()
        {
            string? token = await GetAccessTokenAsync().ConfigureAwait(false);

            return !token.IsNullOrEmpty();
        }

        public static async Task<string?> GetCurrentUserGuidAsync()
        {
            if (_currentUserGuid.IsNotNullOrEmpty())
            {
                return _currentUserGuid;
            }

            _currentUserGuid = await PreferenceGetAsync(ClientNames.CurrentUserGuid).ConfigureAwait(false);

            return _currentUserGuid;
        }

        public static async Task SetCurrentUserGuidAsync(string? userGuid)
        {
            _currentUserGuid = userGuid;
            await PreferenceSetAsync(ClientNames.CurrentUserGuid, userGuid).ConfigureAwait(false);
        }

        public static async Task<string?> GetCurrentLoginNameAsync()
        {
            if (_currentLoginName.IsNullOrEmpty())
            {
                _currentLoginName = await PreferenceGetAsync(ClientNames.CurrentLoginName).ConfigureAwait(false);
            }

            return _currentLoginName;
        }

        public static async Task SetCurrentLoginNameAsync(string? loginName)
        {
            _currentLoginName = loginName;

            await PreferenceSetAsync(ClientNames.CurrentLoginName, loginName).ConfigureAwait(false);
        }

        #endregion

        #region Token

        public static async Task<string?> GetAccessTokenAsync()
        {
            if (_accessToken.IsNotNullOrEmpty())
            {
                return _accessToken!;
            }

            _accessToken = await PreferenceGetAsync(ClientNames.AccessToken).ConfigureAwait(false);

            return _accessToken;
        }

        public static async Task SetAccessTokenAsync(string? accessToken)
        {
            _accessToken = accessToken;
            await PreferenceSetAsync(ClientNames.AccessToken, accessToken).ConfigureAwait(false);
        }

        public static async Task<string?> GetRefreshTokenAsync()
        {
            if (_refreshToken.IsNotNullOrEmpty())
            {
                return _refreshToken;
            }

            _refreshToken = await PreferenceGetAsync(ClientNames.RefreshToken).ConfigureAwait(false);

            return _refreshToken;
        }

        public static async Task SetRefreshTokenAsync(string? refreshToken)
        {
            _refreshToken = refreshToken;
            await PreferenceSetAsync(ClientNames.RefreshToken, refreshToken).ConfigureAwait(false);
        }

        public static async Task OnJwtRefreshSucceedAsync(string newAccessToken)
        {
            await SetAccessTokenAsync(newAccessToken).ConfigureAwait(false);
        }

        public static async Task OnJwtRefreshFailedAsync()
        {
            await SetAccessTokenAsync(null).ConfigureAwait(false);
            await SetRefreshTokenAsync(null).ConfigureAwait(false);
        }

        public static async Task OnLoginSuccessedAsync(string userGuid, string? loginName, string? mobile, string? email, string accessToken, string refreshToken)
        {
            await SetCurrentUserGuidAsync(userGuid).ConfigureAwait(false);
            await SetCurrentLoginNameAsync(loginName).ConfigureAwait(false);
            await SetCurrentEmailAsync(email).ConfigureAwait(false);
            await SetCurrentMobileAsync(mobile).ConfigureAwait(false);

            await SetAccessTokenAsync(accessToken).ConfigureAwait(false);
            await SetRefreshTokenAsync(refreshToken).ConfigureAwait(false);

        }

        public static async Task OnLoginFailedAsync()
        {
            await OnJwtRefreshFailedAsync().ConfigureAwait(false);
        }

        #endregion

        #region Privates

        private static async Task<string?> PreferenceGetAsync(string key)
        {
            if (key.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                string? value = await SecureStorage.GetAsync(key).ConfigureAwait(false);
                return value;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Application.Current.Log(LogLevel.Critical, ex, $"SecureStorage Get 失败，很严重. key:{key}. Message:{ex.Message}");
                return null;
            }
        }

        private static async Task PreferenceSetAsync(string key, string? value)
        {
            if (key.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                await SecureStorage.SetAsync(key, value).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                //TODO: Possible that device doesn't support secure storage on device.
                Application.Current.Log(LogLevel.Critical, ex, $"SecureStorage Set 失败，很严重. key:{key}. Message:{ex.Message}");
                return;
            }
        }

        public static async Task<string?> GetCurrentMobileAsync()
        {
            if (_currentMobile.IsNullOrEmpty())
            {
                _currentMobile = await PreferenceGetAsync(ClientNames.Mobile).ConfigureAwait(false);
            }

            return _currentMobile;
        }

        public static async Task SetCurrentMobileAsync(string? mobile)
        {
            _currentMobile = mobile;

            await PreferenceSetAsync(ClientNames.Mobile, mobile).ConfigureAwait(false);
        }

        public static async Task<string?> GetCurrentEmailAsync()
        {
            if (_currentEmail.IsNullOrEmpty())
            {
                _currentEmail = await PreferenceGetAsync(ClientNames.Email).ConfigureAwait(false);
            }

            return _currentEmail;
        }

        public static async Task SetCurrentEmailAsync(string? email)
        {
            _currentEmail = email;

            await PreferenceSetAsync(ClientNames.Email, email).ConfigureAwait(false);
        }

        #endregion
    }
}
