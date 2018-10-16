using System;
using HB.Component.CentralizedLogger;
using HB.Framework.EventBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;

namespace WebDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var _elasticSettings = new ConnectionSettings(new Uri(Configuration["ElasticSearch:ElasticSearchUri"]));
            var _elasticClient = new ElasticClient(_elasticSettings);
            services.AddSingleton<IElasticClient>(_elasticClient);

            services.AddKafkaEngine(Configuration.GetSection("Kafka"));

            services.AddEventBus(Configuration.GetSection("EventBus"));

            services.AddCentralizedLogger(Configuration.GetSection("CentralizedLog"));

            services.AddCentralizedLoggerEventHandler(Configuration.GetSection("CentralizedLogServer"));

            services.AddAliyunService(Configuration.GetSection("Aliyun"));
            services.AddAliyunSms(Configuration.GetSection("AliyunSms"));

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, CentralizedLoggerProvider centralizedLoggerProvider, IEventBus eventBus)
        {
            loggerFactory.AddCentralizedLog(centralizedLoggerProvider);

            
            eventBus.Handle();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
