using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client;
using HB.FullStack.Common;
using HB.FullStack.XamarinForms.Utils;

using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.XamarinForms
{
    public static class DevicePreferences
    {
        private const int ADDRESS_REQUEST_INTERVAL_SECONDS = 60;

        private static string? _deviceId;
        private static string? _deviceVersion;
        private static DeviceInfos? _deviceInfos;
        private static string? _deviceAddress;

        private static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

        public static string DeviceId
        {
            get
            {
                if (_deviceId.IsNullOrEmpty())
                {
                    string? stored = PreferenceHelper.Get(ClientConventions.PREFERENCE_NAME_DEVICEID);

                    if (stored.IsNullOrEmpty())
                    {
                        stored = MobileUtils.CreateNewDeviceId();
                        PreferenceHelper.Set(ClientConventions.PREFERENCE_NAME_DEVICEID, stored);
                    }

                    _deviceId = stored;
                }

                return _deviceId;
            }
        }

        public static DeviceInfos DeviceInfos
        {
            get
            {
                if (_deviceInfos == null)
                {
                    _deviceInfos = MobileUtils.GetDeviceInfos();
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
                    _deviceVersion = MobileUtils.GetDeviceVersion();
                }

                return _deviceVersion!;
            }
        }

        public static async Task<string> GetDeviceAddressAsync()
        {
            if (_deviceAddress.IsNullOrEmpty() || RequestLocker.NoWaitLock(nameof(DevicePreferences), nameof(GetDeviceAddressAsync), TimeSpan.FromSeconds(ADDRESS_REQUEST_INTERVAL_SECONDS)))
            {
                _deviceAddress = await MobileUtils.GetDeviceAddressAsync().ConfigureAwait(false);
            }

            return _deviceAddress;
        }
    }
}