using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HB.Framework.Client
{
    //TODO: 考虑SecurityStorage不支持时，改用普通的Storage
    public class ClientGlobal : IClientGlobal
    {
        private string? _deviceId;
        private string? _deviceType;
        private string? _deviceVersion;
        private string? _deviceAddress;

        private string? _currentUserGuid;
        private string? _accessToken;
        private string? _refreshToken;

        private readonly ILogger _logger;

        public ClientGlobal(ILogger<ClientGlobal> logger)
        {
            _logger = logger;
        }

        #region Device

        public async Task<string> GetDeviceIdAsync()
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

        public async Task<string> GetDeviceTypeAsync()
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

        public async Task<string> GetDeviceVersionAsync()
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

        public async Task<string> GetDeviceAddressAsync()
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

        public async Task<bool> IsUserLoginedAsync()
        {
            string? token = await GetAccessTokenAsync().ConfigureAwait(false);

            return !token.IsNullOrEmpty();
        }

        public async Task<string?> GetCurrentUserGuidAsync()
        {
            if (_currentUserGuid.IsNotNullOrEmpty())
            {
                return _currentUserGuid;
            }

            _currentUserGuid = await PreferenceGetAsync(ClientNames.CurrentUserGuid).ConfigureAwait(false);

            return _currentUserGuid;
        }

        public async Task SetCurrentUserGuidAsync(string userGuid)
        {
            _currentUserGuid = userGuid;
            await PreferenceSetAsync(ClientNames.CurrentUserGuid, userGuid).ConfigureAwait(false);
        }

        #endregion

        #region Token

        public async Task<string?> GetAccessTokenAsync()
        {
            if (_accessToken.IsNotNullOrEmpty())
            {
                return _accessToken!;
            }

            _accessToken = await PreferenceGetAsync(ClientNames.AccessToken).ConfigureAwait(false);

            return _accessToken;
        }



        public async Task SetAccessTokenAsync(string? accessToken)
        {
            _accessToken = accessToken;
            await PreferenceSetAsync(ClientNames.AccessToken, accessToken).ConfigureAwait(false);
        }

        public async Task<string?> GetRefreshTokenAsync()
        {
            if (_refreshToken.IsNotNullOrEmpty())
            {
                return _refreshToken;
            }

            _refreshToken = await PreferenceGetAsync(ClientNames.RefreshToken).ConfigureAwait(false);

            return _refreshToken;
        }

        public async Task SetRefreshTokenAsync(string? refreshToken)
        {
            _refreshToken = refreshToken;
            await PreferenceSetAsync(ClientNames.RefreshToken, refreshToken).ConfigureAwait(false);
        }

        #endregion

        #region Privates

        private async Task<string?> PreferenceGetAsync(string key)
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
                _logger.LogCritical($"SecureStorage Get Failed. key:{key}. Message:{ex.Message}");
                return null;
            }
        }

        private async Task PreferenceSetAsync(string key, string? value)
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
                _logger.LogCritical($"SecureStorage Set Failed. key:{key}. Message:{ex.Message}");
                return;
            }
        }

        #endregion
    }
}
