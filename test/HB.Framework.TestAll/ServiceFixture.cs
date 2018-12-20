using System;
using System.Collections.Generic;
using System.Text;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace HB.Framework.TestAll
{
    public class ServiceFixture : IDisposable
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
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional:true);


            Configuration = configurationBuilder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddOptions();
 
            serviceCollection.AddLogging(builder => {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddNLog();
            });

            serviceCollection.AddRedisEngine(Configuration.GetSection("RedisEngine"));
            serviceCollection.AddEventBus(Configuration.GetSection("EventBus"));
            serviceCollection.AddRabbitMQEngine(Configuration.GetSection("RabbitMQ"));

            Services = serviceCollection.BuildServiceProvider();
        }

        public IEventBus EventBus => this.Services.GetRequiredService<IEventBus>();

        public ILoggerFactory LoggerFactory => this.Services.GetRequiredService<ILoggerFactory>();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    NLog.LogManager.Shutdown();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ServiceFixture() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
