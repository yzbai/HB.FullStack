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
        public IConfiguration Configuration { get; private set; } = default!;

        public IServiceProvider Services { get; private set; } = default!;

        public ServiceFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);


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

        public IAliyunSmsService SmsService {
            get {
                return Services.GetRequiredService<IAliyunSmsService>();
            }
        }

        public IAliyunOssService AliyunOssService {
            get {
                return Services.GetRequiredService<IAliyunOssService>();
            }
        }
    }
}
