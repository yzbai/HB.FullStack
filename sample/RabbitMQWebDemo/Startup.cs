using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.EventBus.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace RabbitMQWebDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddEventBus(Configuration.GetSection("EventBus"));

            services.AddSingleton<IPublishBiz, PublishBiz>();

            services.AddHostedService<ConsumerService>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IPublishBiz publishBiz)
        {
            for (int i =0; i < 100000; ++i)
            {
                publishBiz.DoSomeWork($"DO some work, this {i}th.");
            }
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }

    public interface IPublishBiz
    {
        void DoSomeWork(string v);
    }

    public class PublishBiz : IPublishBiz
    {
        private IEventBus _eventBus;
        private ILogger _logger;

        public PublishBiz(IEventBus eventBus, ILogger<PublishBiz> logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public void DoSomeWork(string str)
        {
            _logger.LogDebug($"Begin Do some work with {str}");

            //Do some work

            //Publish Done Event

            var eventMsg = new EventMessage(type: "Framework.RabbitMQWebDemo.DoSomework.Done", jsonData: $"true,  {str}");

            _eventBus.PublishAsync(eventMsg);

            _logger.LogDebug($"End Do some work with {str}");
        }
    }

    public class ConsumerService : BackgroundService
    {
        private IEventBus _eventBus;

        public ConsumerService(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return null;
        }
    }

}
