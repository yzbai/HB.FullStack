using HB.FullStack.Common.ApiClient;
using HB.FullStack.Common.Test;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

                    options.LoginJwtEndpoint = new JwtEndpointSetting
                    {
                        EndpointName = ApiEndpointName,
                        ResName = JwtRes,
                        Version = ApiVersion
                    };

                    options.Endpoints.Add(new EndpointSettings
                    {
                        Name = ApiEndpointName,
                        Version = ApiVersion,
                        Url = new Uri($"http://localhost:{Port}/api/"),
                        JwtEndpoint = new JwtEndpointSetting
                        {
                            EndpointName = ApiEndpointName,
                            ResName = JwtRes,
                            Version = ApiVersion
                        }
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
