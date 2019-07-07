using HB.Component.Resource.Vod;
using HB.Component.Resource.Vod.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AliyunSample
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        public static IServiceProvider Services { get; private set; }

        private static void ConfigureServices()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false);

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                configurationBuilder.AddUserSecrets<Program>();
            }

            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => {
                builder.AddConsole();
            });

        
            serviceCollection.AddAliyunSms(Configuration.GetSection("AliyunSms"));
            serviceCollection.AddAliyunVod(Configuration.GetSection("AliyunVod"));

          
            Services = serviceCollection.BuildServiceProvider();
        }



        static void Main(string[] args)
        {
            ConfigureServices();

            VodTest vodTest = ActivatorUtilities.CreateInstance<VodTest>(Services);

            vodTest.Test();
        }



    }

    public class VodTest
    {
        private IVodService _vodService;

        public VodTest(IVodService vodBiz)
        {
            _vodService = vodBiz;  
        }

        public void Test()
        {
            string vid = "93b8be2390b14398b4d6fbff38bf9d4f";

            PlayAuth playAuth = _vodService.GetVideoPlayAuth(vid, 600).Result;

            Console.WriteLine(playAuth.Auth);
        }
    }
}
