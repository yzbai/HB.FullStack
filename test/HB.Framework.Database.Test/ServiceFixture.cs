using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HB.Framework.Database.Test
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false);

            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(builder => {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
                builder.AddDebug();
            });

            //Database
            services.AddMySQL(Configuration.GetSection("MySQL"));

            Services = services.BuildServiceProvider();
        }

        public IDatabase Database => Services.GetRequiredService<IDatabase>();

    }
}
