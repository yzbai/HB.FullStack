using System;
using System.Threading.Tasks;

using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.VisualStudio.Threading;

namespace HB.FullStack.Client.Maui
{
    public static class DevicePreferences
    {
        private const int ADDRESS_REQUEST_INTERVAL_SECONDS = 60;
        public const string PREFERENCE_NAME_DEVICEID = "dbuKErtT";

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
                    string? stored = PreferenceHelper.Get(PREFERENCE_NAME_DEVICEID);

                    if (stored.IsNullOrEmpty())
                    {
                        stored = CreateNewDeviceId();
                        PreferenceHelper.Set(PREFERENCE_NAME_DEVICEID, stored);
                    }

                    _deviceId = stored;
                }

                return _deviceId;
            }
        }

        public static string CreateNewDeviceId()
        {
            return SecurityUtil.CreateUniqueToken();
        }

        public static DeviceInfos DeviceInfos
        {
            get
            {
                if (_deviceInfos == null)
                {
                    _deviceInfos = new DeviceInfos
                    {
                        Name = DeviceInfo.Name ?? "Unkown",
                        Model = DeviceInfo.Model ?? "Unkown",
                        OSVersion = DeviceInfo.VersionString ?? "Unkown",
                        Platform = DeviceInfo.Platform.ToString(),
                        Idiom = DeviceInfo.Idiom.ToString() switch
                        {
                            "Phone" => System.DeviceIdiom.Phone,
                            "Tablet" => System.DeviceIdiom.Tablet,
                            "Desktop" => System.DeviceIdiom.Desktop,
                            "TV" => System.DeviceIdiom.TV,
                            "Watch" => System.DeviceIdiom.Watch,
                            "Web" => System.DeviceIdiom.Web,
                            _ => System.DeviceIdiom.Unknown
                        },
                        Type = DeviceInfo.DeviceType.ToString()
                    };
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
                    _deviceVersion = AppInfo.VersionString;
                }

                return _deviceVersion!;
            }
        }


        public static async Task<string> GetDeviceAddressAsync()
        {
            if (_deviceAddress.IsNullOrEmpty() || RequestLocker.NoWaitLock(nameof(DevicePreferences), nameof(GetDeviceAddressAsync), TimeSpan.FromSeconds(ADDRESS_REQUEST_INTERVAL_SECONDS)))
            {
                _deviceAddress = await GetLastKnownAddressAsync().ConfigureAwait(false);
            }

            return _deviceAddress;
        }

        private static async Task<string> GetLastKnownAddressAsync()
        {
            try
            {
                Location? location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

                if (location != null)
                {
                    return $"{location.Latitude}.{location.Longitude}.{location.Altitude}.{location.Accuracy}.{location.Speed}.{location.Timestamp}";
                }
            }
            catch (FeatureNotSupportedException)
            {
                // Handle not supported on device exception
            }
            catch (FeatureNotEnabledException)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException)
            {
                // Handle permission exception
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // Unable to get location
            }

            return "unkown";
        }
    }
}