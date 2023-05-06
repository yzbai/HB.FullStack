using System;

using HB.FullStack.Server.WebLib.Services;

using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Server.WebLib.Startup
{
    public class WebApiStartupSettings
    {
        public Action<InitServiceOptions> ConfigureInitServiceOptions { get; set; }
        public Action<DirectoryOptions> ConfigureDirectoryOptions { get; }
        public Action<IServiceCollection> ConfigureServices { get; set; }

        public WebApiStartupSettings(Action<IServiceCollection> configureServices, Action<InitServiceOptions> configureInitializationOptions, Action<DirectoryOptions> configureDirectoryTokenOptions)
        {
            ConfigureServices = configureServices;
            ConfigureInitServiceOptions = configureInitializationOptions;
            ConfigureDirectoryOptions = configureDirectoryTokenOptions;
        }

    }
}
