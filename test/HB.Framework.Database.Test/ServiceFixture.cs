using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace HB.Framework.Database.Test
{
    public class ServiceFixture
    {
        public IConfiguration Configuration { get; private set; }

        public IServiceProvider Services { get; private set; }

        public ServiceFixture()
        {
            NLog.LogManager.LoadConfiguration("nlog.config");

            var configurationBuilder = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.Development.json", optional: true);


            Configuration = configurationBuilder.Build();

            IServiceCollection services = new ServiceCollection();

            services.AddOptions();

            services.AddLogging(builder => {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            });


            //Database
            services.AddMySQLEngine(Configuration.GetSection("MySQL"));
            services.AddDatabase(Configuration.GetSection("Database"));

            Services = services.BuildServiceProvider();
        }

        public IDatabase Database => Services.GetRequiredService<IDatabase>();

        public ISQLBuilder SQLBuilder => Services.GetRequiredService<ISQLBuilder>();
    }
}
