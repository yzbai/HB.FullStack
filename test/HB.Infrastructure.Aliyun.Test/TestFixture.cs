using HB.Compnent.Common.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Infrastructure.Aliyun.Test
{
    public class TestFixture : IDisposable
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        public TestFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("Test.json", optional: false);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddAliyunClient(Configuration.GetSection("Aliyun"));
            serviceCollection.AddAliyunSms(Configuration.GetSection("AliyunSms"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public ISmsBiz GetSmsBiz()
        {
            return Services.GetRequiredService<ISmsBiz>();
        }

        public void Dispose()
        {

        }
    }
}
