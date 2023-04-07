using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using HB.FullStack.Web;
using HB.FullStack.Web.Filters;
using HB.FullStack.Web.Security;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

using Polly;

using Serilog;

using StackExchange.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace HB.FullStack.Server.Startup
{
    public static class WebApiStartup
    {
        private const string IdGen                = "IdGen";
        private const string RedisLock            = "RedisLock";
        private const string RedisKVStore         = "RedisKVStore";
        private const string RedisCache           = "RedisCache";
        private const string RedisEventBus        = "RedisEventBus";
        private const string Database             = "Database";
        private const string Identity             = "Identity";
        private const string DataProtection       = "DataProtection";
        private const string JwtAuthentication    = "JwtAuthentication";
        private const string ApiKeyAuthentication = "ApiKeyAuthentication";
        private const string TCaptha              = "TCaptha";
        private const string AliyunSts            = "AliyunSts";
        private const string AliyunSms            = "AliyunSms";

        public static IConfiguration Configuration = null!;
        public static void Run(string[] args, WebApiStartupSettings settings)
        {
            try
            {
                //Log & Environment
                SerilogHelper.OpenLogs();
                EnvironmentUtil.EnsureEnvironment();
                Globals.Logger.LogStarup();

                //Builder
                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                Configuration = builder.Configuration;

                ConfigureBuilder(builder, settings);

                //App
                WebApplication app = builder.Build();
                GlobalWebApplicationAccessor.Application = app;
                ConfigureApplication(app, settings);

                //Run
                app.Run();
            }
            catch (Exception ex)
            {
                Globals.Logger.LogCriticalShutDown(ex);
            }
            finally
            {
                SerilogHelper.CloseLogs();
            }
        }

        private static void ConfigureBuilder(WebApplicationBuilder builder, WebApiStartupSettings settings)
        {
            builder.Host.UseSerilog();

            IServiceCollection services = builder.Services;

            services.AddModelDefFactory();
            services.AddMemoryLock();
            services.AddIdGen(Configuration.GetSection(IdGen));

            if (settings.UseDistributedLock)
            {
                services.AddSingleRedisDistributedLock(Configuration.GetSection(RedisLock));
            }

            if (settings.UseKVStore)
            {
                services.AddRedisKVStore(Configuration.GetSection(RedisKVStore));
            }

            if (settings.UseCache)
            {
                services.AddRedisCache(Configuration.GetSection(RedisCache));
            }

            if (settings.UseEventBus)
            {
                services.AddRedisEventBus(Configuration.GetSection(RedisEventBus));
            }

            if (settings.UseDatabase)
            {
                services.AddDatabase(Configuration.GetSection(Database), builder => builder.AddMySQL());
            }

            if (settings.UseIdentity)
            {
                services.AddIdentity(Configuration.GetSection(Identity));
            }

            if (settings.UseCaptha)
            {
                services.AddTCaptha(Configuration.GetSection(TCaptha));
            }

            if (settings.UseAliyunSms)
            {
                services.AddAliyunSts(Configuration.GetSection(AliyunSts));
                services.AddAliyunSms(Configuration.GetSection(AliyunSms));
            }

            //DataProtection
            services.AddDataProtectionWithCertInRedis(settings => Configuration.GetSection(DataProtection).Bind(settings));

            //Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtAuthentication(
                    configureJwtClientSettings: settings => Configuration.GetSection(JwtAuthentication).Bind(settings),
                    onChallenge: OnJwtChallengeAsync,
                    onTokenValidated: OnJwtTokenValidatedAsync,
                    onAuthenticationFailed: OnJwtAuthenticationFailedAsync,
                    onForbidden: OnJwtForbiddenAsync,
                    onMessageReceived: OnJwtMessageReceivedAsync)
                .AddApiKeyAuthentication(options => Configuration.GetSection(ApiKeyAuthentication).Bind(options));

            //Authorization
            services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            });

            //Controller
            services.AddControllersWithConfiguration(addAuthentication: true);
            services.AddSwaggerGen();

            //Proxy
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                //这里是加nginx服务器的ip，默认是127.0.0.1 当nginx服务器不在同一台物理服务器上时使用
                //https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-5.0
                //options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            });


            //HB.FullStack.Web Services
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<ICommonResourceTokenService, CommonResourceTokenService>();
            services.AddScoped<UserActivityFilter>();
            services.AddScoped<CheckCommonResourceTokenFilter>();

            //InitHostedService
            services
                .Configure(settings.ConfigureInitHostedServiceOptions)
                .AddHostedService<InitHostedService>();

            //User Settings
            settings.ConfigureServices(services);
        }


        static IServiceCollection AddDataProtectionWithCertInRedis(this IServiceCollection services, Action<DataProtectionSettings> action)
        {
            DataProtectionSettings dataProtectionSettings = new DataProtectionSettings();
            action(dataProtectionSettings);

            string redisKey = $"{dataProtectionSettings.ApplicationName}_{EnvironmentUtil.AspNetCoreEnvironment}_dpk";

            X509Certificate2 certificate2 = CertificateUtil.GetCertificateFromSubjectOrFile(
                dataProtectionSettings.CertificateSubject,
                dataProtectionSettings.CertificateFileName,
                dataProtectionSettings.CertificateFilePassword);

            ConfigurationOptions redisConfigurationOptions = ConfigurationOptions.Parse(dataProtectionSettings.RedisConnectString);
            redisConfigurationOptions.AllowAdmin = false;

            Policy
                .Handle<RedisConnectionException>()
                .WaitAndRetryForever(
                            count => TimeSpan.FromSeconds(5 + count * 2),
                            (exception, retryCount, timeSpan) =>
                            {
                                RedisConnectionException ex = (RedisConnectionException)exception;
                                Log.Fatal(
                                    exception,
                                    $"DataProtection : Try {retryCount}th times. Wait For {timeSpan.TotalSeconds} seconds. Redis Can not connect {dataProtectionSettings.RedisConnectString} : {redisKey};"
                                );
                            })
                .Execute(() =>
                {
                    ConnectionMultiplexer redisMultiplexer = ConnectionMultiplexer.Connect(redisConfigurationOptions);

                    services
                        .AddDataProtection()
                        .SetApplicationName(dataProtectionSettings.ApplicationName)
                        .ProtectKeysWithCertificate(certificate2)
                        .PersistKeysToStackExchangeRedis(redisMultiplexer, redisKey);
                });

            return services;
        }

        static IServiceCollection AddControllersWithConfiguration(this IServiceCollection services, bool addAuthentication = true)
        {
            Assembly httpFrameworkAssembly = typeof(GlobalExceptionController).Assembly;

            //authenticationBuilder.AddTransient<ProblemDetailsFactory, CustomProblemDetailsFactory>();

            services
                .AddControllers(options =>
                {
                    if (addAuthentication)
                    {
                        //need authenticated by default. no need add [Authorize] everywhere
                        AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                        options.Filters.Add(new AuthorizeFilter(policy));

                        //options.Filters
                        options.Filters.AddService<UserActivityFilter>();
                    }
                })
                .AddJsonOptions(options =>
                {
                    SerializeUtil.Configure(options.JsonSerializerOptions);
                })
                .ConfigureApiBehaviorOptions(apiBehaviorOptions =>
                {
                    apiBehaviorOptions.InvalidModelStateResponseFactory = actionContext =>
                    {
                        ErrorCode errorCode = ErrorCodes.ModelValidationError.WithMessage(actionContext.ModelState.GetErrors());

                        return new BadRequestObjectResult(errorCode)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };
                })
                .PartManager.ApplicationParts.Add(new AssemblyPart(httpFrameworkAssembly));

            services.AddEndpointsApiExplorer();

            return services;
        }

        private static void ConfigureApplication(WebApplication app, WebApiStartupSettings settings)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            //TODO: 使用RateLimiting

            app
                .UseExceptionController()
                .UseForwardedHeaders()
                .UseHttpMethodOverride();

            if (!app.Environment.IsDevelopment())
            {
                app.UseHsts();
            }

            app
                .Use((httpContext, next) =>
                {
                    httpContext.Request.EnableBuffering();
                    return next();
                })

                //.UseDefaultFiles()
                .UseStaticFiles()

                .UseSerilogRequestLogging()

                //使用了oss,不使用本地文件系统
                //.UseStaticFiles(new StaticFileOptions(new SharedOptions
                //{
                //    FileProvider = new PhysicalFileProvider(serverOptions.FileSettings.PublicPath),
                //    RequestPath = "/Files/Public"
                //}))

                //TODO: 使用Nginx时注释掉，Behind Nginx on same machine
                .UseOnlyHttps()

                //这个很重要，不添加就会被默认加到最开头
                .UseRouting()

                .UseCors()
                .UseAuthentication()
                .UseAuthorization();

            //使用了oss,不使用本地文件系统
            //.UseStaticFiles(new StaticFileOptions(new SharedOptions
            //{
            //    FileProvider = new PhysicalFileProvider(serverOptions.FileSettings.ProtectedPath),
            //    RequestPath = "/Files/Protected"
            //}))

            //.net 5, .net 6 add this by default
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllers();
            //});

            //.net 6
            app.MapControllers();
        }

        #region Jwt Actions

        private static Task OnJwtMessageReceivedAsync(MessageReceivedContext arg)
        {
            return Task.CompletedTask;
        }

        private static Task OnJwtForbiddenAsync(ForbiddenContext arg)
        {
            return Task.CompletedTask;
        }

        private static Task OnJwtAuthenticationFailedAsync(AuthenticationFailedContext arg)
        {
            //TODO: 说明这个AccessToken有风险，应该拒绝他的刷新。Black相应的RefreshToken
            return Task.CompletedTask;
        }

        private static Task OnJwtTokenValidatedAsync(TokenValidatedContext c)
        {
            //验证Body 中的ClientId 与 JWT 中的ClientId 是否一致
            string? jwtClientId = c.Principal?.GetClientId();

            string? requestClientId = c.HttpContext.Request.GetHeaderValueAs<string>(ClientNames.CLIENT_ID);

            requestClientId ??= c.HttpContext.Request.GetValue(ClientNames.CLIENT_ID);

            if (!string.IsNullOrWhiteSpace(jwtClientId) && jwtClientId.Equals(requestClientId, Globals.ComparisonIgnoreCase))
            {
                return Task.CompletedTask;
            }

            c.Fail("Token ClientId do not match Request ClientId");

            Log.Warning($"ClientId:{requestClientId} do not match Request ClientId : {jwtClientId}");

            return Task.CompletedTask;
        }

        private static Task OnJwtChallengeAsync(JwtBearerChallengeContext c)
        {
            //c.HandleResponse();
            //c.Response.StatusCode = 401;

            //c.Response.ContentType = "application/problem+json";
            //ErrorCode error = c.AuthenticateFailure switch
            //{
            //    SecurityTokenExpiredException => ApiErrorCodes.AccessTokenExpired,
            //    _ => ApiErrorCodes.NoAuthority
            //};

            //ErrorCode errorResponse = error.AppendDetail($"Exception:{c.AuthenticateFailure?.Description}, Error:{c.Error}, ErrorDescription:{c.ErrorDescription}, ErrorUri:{c.ErrorUri}");

            //return c.Response.WriteAsync(SerializeUtil.ToJson(errorResponse));

            //如果采用写到Respose的body中去的，就会阻断pipeline，直接返回了，影响了ExceptionHandler的接收

            //TODO: 是否直接抛出异常？
            c.HandleResponse();

            c.Response.StatusCode = 401;

            ErrorCode error = c.AuthenticateFailure switch
            {
                SecurityTokenExpiredException => ErrorCodes.AccessTokenExpired,
                _ => ErrorCodes.NoAuthority
            };

            //error.AppendDetail($"Exception:{c.AuthenticateFailure?.Description}, Error:{c.Error}, ErrorDescription:{c.ErrorDescription}, ErrorUri:{c.ErrorUri}");

            c.Response.Headers.Append(HeaderNames.WWWAuthenticate, $"{c.Options.Challenge} {SerializeUtil.ToJson(error)}");

            return Task.CompletedTask;
        }

        #endregion
    }
}
