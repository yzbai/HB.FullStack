using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Mobile.Utils;
using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Mobile
{
    public static class DevicePreferences
    {
        private static string? _deviceId;
        private static string? _deviceVersion;
        private static DeviceInfos? _deviceInfos;
        private static string? _deviceAddress;

        public static async Task<string> GetDeviceIdAsync()
        {
            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId!;
            }

            _deviceId = await PreferenceHelper.PreferenceGetAsync(nameof(_deviceId)).ConfigureAwait(false);

            if (_deviceId.IsNotNullOrEmpty())
            {
                return _deviceId!;
            }

            _deviceId = ClientUtils.CreateNewDeviceId();

            await PreferenceHelper.PreferenceSetAsync(nameof(_deviceId), _deviceId).ConfigureAwait(false);

            return _deviceId!;
        }

        public static string GetDeviceId()
        {
            if (_deviceId.IsNullOrEmpty())
            {
                using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
                JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

                return joinableTaskFactory.Run(async () => { return await GetDeviceIdAsync().ConfigureAwait(false); });
            }

            return _deviceId!;
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
            //TODO:隔一段时间取一次地理位置
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

        public static string GetDeviceAddress()
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
}
