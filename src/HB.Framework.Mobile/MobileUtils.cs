using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HB.Framework.Mobile
{
    public class MobileUtils
    {
        public static string GetDeviceType()
        {
            return $"{DeviceInfo.DeviceType}.{DeviceInfo.Idiom}.{DeviceInfo.Manufacturer}.{DeviceInfo.Model}.{DeviceInfo.Name}.{DeviceInfo.Platform}.{DeviceInfo.VersionString}";
        }

        public static string GetDeviceVersion()
        {
            return AppInfo.VersionString;
        }

        public static async Task<string> GetDeviceAddressAsync()
        {
            Location location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

            return $"{location.Latitude}.{location.Longitude}.{location.Altitude}.{location.Accuracy}.{location.Speed}.{location.Timestamp}";
        }

        public static string CreateNewDeviceId()
        {
            return SecurityUtil.CreateUniqueToken();
        }

        public static IConfiguration BuildConfiguration(string appsettingsFileName, [ValidatedNotNull]Assembly executingAssembly)
        {
            //TODO: 性能与安全检测
            string fullPath = Path.Combine(FileSystem.CacheDirectory, appsettingsFileName);

            using (Stream resFileStream = executingAssembly.GetManifestResourceStream(appsettingsFileName))
            {
                if (resFileStream != null)
                {
                    using FileStream fileStream = File.Create(fullPath);
                    resFileStream.CopyTo(fileStream);
                }
            }

            IConfigurationBuilder builder = new ConfigurationBuilder();

            builder.AddJsonFile(fullPath, false);

            return builder.Build();
        }
    }
}
