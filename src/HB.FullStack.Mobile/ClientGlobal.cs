using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Client
{
    public enum ClientState
    {
        NotLogined,
        NewVersionAndLogined,
        OldVersionAndLogined
    }

    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public static class ClientGlobal
    {
        private static string? _deviceId;
        private static DeviceInfos? _deviceInfos;
        private static string? _deviceVersion;
        private static string? _deviceAddress;

        private static string? _currentUserGuid;
        private static string? _currentLoginName;
        private static string? _currentMobile;
        private static string? _currentEmail;
        private static string? _accessToken;
        private static string? _refreshToken;

        private static bool? _isLogined;

        #region Const

        public const int SmsCodeLength = 6;

        public const string EffectsGroupName = "HB.FullStack.Client.Effects";

        #endregion

        #region facilities

        //private static readonly MemoryFrequencyChecker _frequencyChecker = new MemoryFrequencyChecker();

        //private const string _apiResourceType = "Api";

        //private static readonly TimeSpan _apiReousrceAliveTimespan = TimeSpan.FromMinutes(2);

        //public static bool CheckSyncFrequency(string userGuid, string resourceName)
        //{
        //    return _frequencyChecker.Check(_apiResourceType + userGuid + resourceName, _apiReousrceAliveTimespan);
        //}

        //public static void ResetSyncFrequency(string userGuid, string resourceName)
        //{
        //    _frequencyChecker.Reset(_apiResourceType, userGuid + resourceName);
        //}

        #endregion

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

        public static string DeviceId
        {
            get
            {
                if (_deviceId.IsNullOrEmpty())
                {
                    using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
                    JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

                    return joinableTaskFactory.Run(async () => { return await GetDeviceIdAsync().ConfigureAwait(false); });
                }

                return _deviceId!;
            }
        }

        public static DeviceInfos DeviceInfos
        {
            get
            {
                if (_deviceInfos == null)
                {
                    _deviceInfos = ClientUtils.GetDeviceInfos();
                }

                return _deviceInfos!;
            }
        }

        public static string DeviceVersion
        {
            get
            {
                if (_deviceVersion.IsNullOrEmpty())
                {
                    _deviceVersion = ClientUtils.GetDeviceVersion();
                }

                return _deviceVersion!;
            }
        }

        public static async Task<string> GetDeviceAddressAsync()
        {
            if (_deviceAddress.IsNotNullOrEmpty())
            {
                return _deviceAddress!;
            }

            //_deviceAddress = await PreferenceGetAsync(ClientNames.DeviceAddress).ConfigureAwait(false);

            //if (_deviceAddress.IsNotNullOrEmpty())
            //{
            //    return _deviceAddress!;
            //}

            _deviceAddress = await ClientUtils.GetDeviceAddressAsync().ConfigureAwait(false);

            //await PreferenceSetAsync(ClientNames.DeviceAddress, _deviceAddress).ConfigureAwait(false);

            return _deviceAddress!;
        }

        public static string DeviceAddress
        {
            get
            {
                if (_deviceAddress.IsNullOrEmpty())
                {
                    using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
                    JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

                    return joinableTaskFactory.Run(async () => { return await GetDeviceAddressAsync().ConfigureAwait(false); });
                }

                return _deviceAddress!;
            }
        }

        #endregion

        #region User

        public static bool IsLogined
        {
            get
            {
                if (_isLogined == null)
                {
                    using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
                    JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

                    return joinableTaskFactory.Run(async () => { return await IsLoginedAsync().ConfigureAwait(false); });
                }

                return _isLogined.Value;
            }
        }

        public static async Task<bool> IsLoginedAsync()
        {
            if (_isLogined == null)
            {
                string? token = await GetAccessTokenAsync().ConfigureAwait(false);

                _isLogined = !token.IsNullOrEmpty();
            }

            return _isLogined.Value;
        }

        public static string? CurrentUserGuid
        {
            get
            {
                if (_currentUserGuid == null)
                {
                    using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
                    JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

                    return joinableTaskFactory.Run(async () => { return await GetCurrentUserGuidAsync().ConfigureAwait(false); });
                }

                return _currentUserGuid;
            }
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

            _isLogined = false;
        }

        public static async Task OnLoginSuccessedAsync(string userGuid, string? loginName, string? mobile, string? email, string accessToken, string refreshToken)
        {
            await SetCurrentUserGuidAsync(userGuid).ConfigureAwait(false);
            await SetCurrentLoginNameAsync(loginName).ConfigureAwait(false);
            await SetCurrentEmailAsync(email).ConfigureAwait(false);
            await SetCurrentMobileAsync(mobile).ConfigureAwait(false);

            await SetAccessTokenAsync(accessToken).ConfigureAwait(false);
            await SetRefreshTokenAsync(refreshToken).ConfigureAwait(false);

            _isLogined = true;
        }

        public static async Task OnLoginFailedAsync()
        {
            await OnJwtRefreshFailedAsync().ConfigureAwait(false);

            _isLogined = false;
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
                await SecureStorage.SetAsync(key, value ?? string.Empty).ConfigureAwait(false);
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
