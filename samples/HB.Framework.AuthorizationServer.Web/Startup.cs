using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using NLog.Extensions.Logging;
using NLog.Web;
using HB.Framework.Common;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using HB.Framework.Http.Security;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HB.Framework.AuthorizationServer.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            Environment = hostingEnvironment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //Options
            services.AddOptions();

            //DataProtection
            services.AddDataProtection()
                .SetApplicationName(Configuration["DataProtection:ApplicationDataProtectionDiscriminator"])
                .PersistKeysToFileSystem(new DirectoryInfo(Configuration["DataProtection:Directory"]))
                .ProtectKeysWithCertificate(Configuration["DataProtection:CertificationThumbPrint"]);

            //Database
            services.AddMySQLEngine(Configuration.GetSection("MySQL"));
            services.AddDatabase(Configuration.GetSection("Database"));

            //KVStore & Cache
            services.AddRedisEngine(Configuration.GetSection("Redis"));
            services.AddKVStore(Configuration.GetSection("KVStore"));
            services.AddDistributedRedisCache(o => {
                Configuration.GetSection("RedisCache").Bind(o);
            });

            //Session
            services.AddMixedSession(o => {  });

            //Aliyun Client
            services.AddAliyunClient(Configuration.GetSection("Aliyun"));

            //Sms Service
            services.AddAliyunSms(Configuration.GetSection("AliyunSms"));

            //Drawing 
            services.AddImageMagick();

            services.AddImageCode();

            //Identity
            services.AddUserIdentity(o => { });

            //For Authentication Server
            services.AddAuthorizationServer(Configuration.GetSection("AuthenticationServer"));

            //For Authentication Client 
            services.AddAuthentication(o => {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(o => {
                o.RequireHttpsMetadata = false;                                 //Only For Develope
                o.Authority = Configuration["AuthenticationClient:Authority"];  //对于Auth Client来说，指Auth Server
                o.Audience = Configuration["AuthenticationClient:Audience"];    //对于Auth Client 来说，指自己
                o.Events = new JwtBearerEvents(){
                    OnChallenge = c => 
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = 401;

                        if (c.Request.Path.StartsWithSegments("/api"))
                        {
                            c.Response.ContentType = "application/json";

                            var rtObj = new { Message = InvalidTokenErrorMessages.GetErrorMessage(c.AuthenticateFailure) };
                            return c.Response.WriteAsync(DataConverter.ToJson(rtObj));
                        }
                        else
                        {
                            //Behave the redirect.
                            return Task.CompletedTask;
                        }
                    }
                };
            });

            //MVC
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IOptions<AuthorizationServerOptions> authServerOptions)
        {

            string tmp = JsonConvert.SerializeObject(authServerOptions.Value.OpenIdConnectConfiguration);
            string tmp2 = JsonConvert.SerializeObject(authServerOptions.Value.JsonWebKeys);
   

            //Logger
            env.ConfigureNLog("nlog.config");
            loggerFactory.AddNLog();

            ILogger logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation($"The Environment is {env.EnvironmentName}.");

            //Exception
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            //Session
            app.UseMixedSession();

            //Static File
            app.UseStaticFiles();

            //For Authentication Client
            app.UseAuthentication();

            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
