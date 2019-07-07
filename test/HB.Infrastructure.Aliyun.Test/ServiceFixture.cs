using HB.Infrastructure.Aliyun.Oss;
using HB.Infrastructure.Aliyun.Sms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace HB.Infrastructure.Aliyun.Test
{
    public class ServiceFixture
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional:true);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

            serviceCollection.AddDistributedMemoryCache();

            serviceCollection.AddAliyunSms(Configuration.GetSection("AliyunSms"));
            serviceCollection.AddAliyunOss(Configuration.GetSection("AliyunOss"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IAliyunSmsService SmsService => Services.GetRequiredService < IAliyunSmsService>();

        public IAliyunOssService AliyunOssService => Services.GetRequiredService<IAliyunOssService>();
    }
}
