using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HB.Framework.Client
{
    public class ClientUtils
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

        public static IConfiguration BuildConfiguration(string appsettingsName, [ValidatedNotNull]Assembly executingAssembly)
        {
            ThrowIf.NullOrEmpty(appsettingsName, nameof(appsettingsName));
            ThrowIf.Null(executingAssembly, nameof(executingAssembly));

            string fileName = $"{executingAssembly.FullName.Split(",")[0]}.{appsettingsName}";

            string fullPath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (Stream resFileStream = executingAssembly.GetManifestResourceStream(fileName))
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
