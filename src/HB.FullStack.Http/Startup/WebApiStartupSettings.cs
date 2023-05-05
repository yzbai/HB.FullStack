using System;

using HB.FullStack.Server.WebLib.Services;

using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Server.WebLib.Startup
{
    public class WebApiStartupSettings
    {
        public bool UseDatabase { get; set; } = true;

        public bool UseKVStore { get; set; } = true;

        public bool UseIdentity { get; set; } = true;

        public bool UseCache { get; set; } = true;

        public bool UseEventBus { get; set; } = true;

        public bool UseDistributedLock { get; set; } = true;

        public bool UseCaptha { get; set; } = true;

        public bool UseAliyunSms { get; set; } = true;

        public Action<InitServiceOptions> ConfigureInitHostedServiceOptions { get; set; }
        public Action<DirectoryTokenOptions> ConfigureDirectoryTokenOptions { get; }
        public Action<IServiceCollection> ConfigureServices { get; set; }

        public WebApiStartupSettings(Action<IServiceCollection> configureServices, Action<InitServiceOptions> configureInitializationOptions, Action<DirectoryTokenOptions> configureDirectoryTokenOptions)
        {
            ConfigureServices = configureServices;
            ConfigureInitHostedServiceOptions = configureInitializationOptions;
            ConfigureDirectoryTokenOptions = configureDirectoryTokenOptions;
        }

    }
}
