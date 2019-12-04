using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HB.Framework.Mobile
{
    public class MobileGlobal : IMobileGlobal
    {
        private readonly ILogger _logger;

        private string _deviceId;
        private string _deviceType;
        private string _deviceVersion;
        private string _deviceAddress;

        private string _currentUserGuid;
        private string _accessToken;
        private string _refreshToken;

        public MobileGlobal(ILogger<MobileGlobal> logger)
        {
            _logger = logger;
        }

        #region Device

        public async Task<string> GetDeviceIdAsync()
        {
            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId;
            }

            _deviceId = await PreferenceGetAsync(MobileNames.DeviceId).ConfigureAwait(false);

            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId;
            }

            _deviceId = MobileUtils.CreateNewDeviceId();

            await PreferenceSetAsync(MobileNames.DeviceId, _deviceId).ConfigureAwait(false);

            return _deviceId;
        }

        public async Task<string> GetDeviceTypeAsync()
        {
            if (_deviceType.IsNotNullOrEmpty())
            {
                return _deviceType;
            }

            _deviceType = await PreferenceGetAsync(MobileNames.DeviceType).ConfigureAwait(false);

            if (_deviceType.IsNotNullOrEmpty())
            {
                return _deviceType;
            }

            _deviceType = MobileUtils.GetDeviceType();

            await PreferenceSetAsync(MobileNames.DeviceType, _deviceType).ConfigureAwait(false);

            return _deviceType;
        }

        public async Task<string> GetDeviceVersionAsync()
        {
            if (_deviceVersion.IsNotNullOrEmpty())
            {
                return _deviceVersion;
            }

            _deviceVersion = await PreferenceGetAsync(MobileNames.DeviceVersion).ConfigureAwait(false);

            if (_deviceVersion.IsNotNullOrEmpty())
            {
                return _deviceVersion;
            }

            _deviceVersion = MobileUtils.GetDeviceVersion();

            await PreferenceSetAsync(MobileNames.DeviceVersion, _deviceVersion).ConfigureAwait(false);

            return _deviceVersion;
        }

        public async Task<string> GetDeviceAddressAsync()
        {
            if (_deviceAddress.IsNotNullOrEmpty())
            {
                return _deviceAddress;
            }

            _deviceAddress = await PreferenceGetAsync(MobileNames.DeviceAddress).ConfigureAwait(false);

            if (_deviceAddress.IsNotNullOrEmpty())
            {
                return _deviceAddress;
            }

            _deviceAddress = await MobileUtils.GetDeviceAddressAsync().ConfigureAwait(false);

            await PreferenceSetAsync(MobileNames.DeviceAddress, _deviceAddress).ConfigureAwait(false);

            return _deviceAddress;
        }

        #endregion

        #region User

        public async Task<bool> IsUserLoginedAsync()
        {
            string token = await GetAccessTokenAsync().ConfigureAwait(false);

            return !token.IsNullOrEmpty();
        }

        public async Task<string> GetCurrentUserGuidAsync()
        {
            if (_currentUserGuid.IsNotNullOrEmpty())
            {
                return _currentUserGuid;
            }

            _currentUserGuid = await PreferenceGetAsync(MobileNames.CurrentUserGuid).ConfigureAwait(false);

            return _currentUserGuid;
        }

        public async Task SetCurrentUserGuidAsync(string userGuid)
        {
            _currentUserGuid = userGuid;
            await PreferenceSetAsync(MobileNames.CurrentUserGuid, userGuid).ConfigureAwait(false);
        }

        #endregion

        #region Token

        public async Task<string> GetAccessTokenAsync()
        {
            if (_accessToken.IsNotNullOrEmpty())
            {
                return _accessToken;
            }

            _accessToken = await PreferenceGetAsync(MobileNames.AccessToken).ConfigureAwait(false);

            return _accessToken;
        }

        public async Task SetAccessTokenAsync(string accessToken)
        {
            _accessToken = accessToken;
            await PreferenceSetAsync(MobileNames.AccessToken, accessToken).ConfigureAwait(false);
        }

        public async Task<string> GetRefreshTokenAsync()
        {
            if (_refreshToken.IsNotNullOrEmpty())
            {
                return _refreshToken;
            }

            _refreshToken = await PreferenceGetAsync(MobileNames.RefreshToken).ConfigureAwait(false);

            return _refreshToken;
        }

        public async Task SetRefreshTokenAsync(string refreshToken)
        {
            _refreshToken = refreshToken;
            await PreferenceSetAsync(MobileNames.RefreshToken, refreshToken).ConfigureAwait(false);
        }

        #endregion

        #region Privates

        private async Task<string> PreferenceGetAsync(string key)
        {
            if (key.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                string value = await SecureStorage.GetAsync(key).ConfigureAwait(false);
                return value;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SecureStorage Get Failed. key:{key}. Message:{ex.Message}");
                return null;
            }
        }

        private async Task PreferenceSetAsync(string key, string value)
        {
            if (key.IsNullOrEmpty() || value.IsNullOrEmpty())
            {
                return;
            }

            try
            {
                await SecureStorage.SetAsync(key, value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                //TODO: Possible that device doesn't support secure storage on device.
                _logger.LogError($"SecureStorage Set Failed. key:{key}. Message:{ex.Message}");
                return;
            }
        }

        #endregion
    }
}
