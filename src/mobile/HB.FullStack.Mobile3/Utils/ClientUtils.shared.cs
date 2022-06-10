using Microsoft;
using Microsoft.Extensions.Configuration;

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace HB.FullStack.Mobile
{
    public static class ClientUtils
    {
        public static DeviceInfos GetDeviceInfos()
        {
            return new DeviceInfos
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

        public static string GetDeviceVersion()
        {
            return AppInfo.VersionString;
        }

        public static async Task<string> GetDeviceAddressAsync()
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
            catch (Exception)
            {
                // Unable to get location
            }

            return "unkown";
        }

        public static string CreateNewDeviceId()
        {
            return SecurityUtil.CreateUniqueToken();
        }

        public static IConfiguration GetConfiguration(string appsettingsFile, [ValidatedNotNull] Assembly executingAssembly)
        {
            ThrowIf.Empty(appsettingsFile, nameof(appsettingsFile));

            string fileName = $"{executingAssembly.FullName!.Split(',')[0]}.{appsettingsFile}";

            using Stream resFileStream = executingAssembly.GetManifestResourceStream(fileName);

            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonStream(resFileStream);

            return builder.Build();
        }
    }
}
