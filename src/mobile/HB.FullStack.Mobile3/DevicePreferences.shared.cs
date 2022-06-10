using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Mobile.Utils;

using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Mobile
{
    public static class DevicePreferences
    {
        private const string DeviceId_Preference_Name = "dbuKErtT";
        private const int Address_Request_Interval_Seconds = 60;

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
                    string? stored = PreferenceHelper.PreferenceGetAsync(DeviceId_Preference_Name).Result;

                    if (stored.IsNullOrEmpty())
                    {
                        stored = ClientUtils.CreateNewDeviceId();
                        PreferenceHelper.PreferenceSetAsync(DeviceId_Preference_Name, stored).Wait();
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
            if (_deviceAddress.IsNullOrEmpty() || RequestLocker.NoWaitLock(nameof(DevicePreferences), nameof(GetDeviceAddressAsync), TimeSpan.FromSeconds(Address_Request_Interval_Seconds)))
            {
                _deviceAddress = await ClientUtils.GetDeviceAddressAsync().ConfigureAwait(false);
            }

            return _deviceAddress;
        }
    }
}
