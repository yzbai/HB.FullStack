using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Files;

using Microsoft.Extensions.Configuration;

namespace Todo.Shared
{
    public class SharedSettings
    {
        public IList<DirectoryDescription> DirectoryDescriptions { get; set; } = new List<DirectoryDescription>();

        public IList<DirectoryPermission> DirectoryPermissions { get; set; } = new List<DirectoryPermission>();

        public static SharedSettings GetSharedSettings(string environment)
        {
            Assembly sharedAssembly = Assembly.GetAssembly(typeof(SharedSettings))!;
            using Stream sharedSettingsStream = sharedAssembly.GetManifestResourceStream("Todo.Shared.SharedSettings.json").ThrowIfNull("no sharedsettings.json");
            using Stream sharedEnvironmentSettingsStream = sharedAssembly.GetManifestResourceStream($"Todo.Shared.SharedSettings.{environment}.json").ThrowIfNull($"no {environment} sharedsettings.json");

            var configBuilder = new ConfigurationBuilder()
                .AddJsonStream(sharedSettingsStream)
                .AddJsonStream(sharedEnvironmentSettingsStream);

            SharedSettings sharedSettings = new SharedSettings();

            configBuilder.Build().Bind(sharedSettings);

            return sharedSettings;
        }
    }
}
