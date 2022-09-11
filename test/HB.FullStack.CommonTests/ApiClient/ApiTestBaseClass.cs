using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Test;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.ApiClient
{
    [TestClass]
    public class ApiTestBaseClass
    {
        public static IApiClient ApiClient { get; set; } = null!;

        public static IPreferenceProvider PreferenceProvider { get; set; } = null!;

        [AssemblyInitialize]
        public static Task AssemblyInit(TestContext _)
        {
            IServiceCollection services = new ServiceCollection();

            services
                .AddOptions()
                .AddLogging(builder => { builder.AddConsole(); })
                .AddSingleton<IPreferenceProvider, PreferenceProviderStub>()
                .AddApiClient(options =>
                {
                    options.HttpClientTimeout = TimeSpan.FromSeconds(100);

                    options.SiteSettings.Add(new SiteSetting
                    {
                        SiteName = ApiEndpointName,
                        Version = ApiVersion,
                        BaseUrl = new Uri($"http://localhost:{Port}/api/"),
                        Endpoints = new List<ResEndpoint> { }
                    });
                });

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            GlobalSettings.Logger = serviceProvider.GetRequiredService<ILogger<ApiTestBaseClass>>();

            ApiClient = serviceProvider.GetRequiredService<IApiClient>();

            PreferenceProvider = serviceProvider.GetRequiredService<IPreferenceProvider>();

            return Task.CompletedTask;
        }

        public static TestHttpServer StartHttpServer(params TestRequestHandler[] handlers)
        {
            TestHttpServer httpServer = new TestHttpServer(Port, new List<TestRequestHandler>(handlers));

            return httpServer;
        }
    }
}
