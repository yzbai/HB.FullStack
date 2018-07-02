using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net;

namespace HB.Framework.AuthorizationServer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
#if DEBUG
            TelemetryConfiguration.Active.DisableTelemetry = true;
#endif

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) => WebHost
            //this auto add appsettings.json & user secrets
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
#if DEBUG
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 56297);
            })
            //.UseKestrel().UseUrls("http://*:56297")
#endif
            .ConfigureLogging((context, loggingBuilder) =>
            {
                loggingBuilder.AddConfiguration(context.Configuration.GetSection("Logging"));
                if (!context.HostingEnvironment.IsProduction())
                {
                    loggingBuilder.AddEventSourceLogger();
                    loggingBuilder.AddDebug();
                }
                //loggingBuilder.AddConsole();
            })
            .Build();
    }
}
