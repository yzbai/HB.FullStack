using System;


using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Server.Startup
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

        public Action<InitHostedServiceOptions> ConfigureInitHostedServiceOptions { get; set; }

        public Action<IServiceCollection> ConfigureServices { get; set; }

        public WebApiStartupSettings(Action<IServiceCollection> configureServices, Action<InitHostedServiceOptions> configureInitializationOptions)
        {
            ConfigureServices = configureServices;
            ConfigureInitHostedServiceOptions = configureInitializationOptions;
        }

    }
}
