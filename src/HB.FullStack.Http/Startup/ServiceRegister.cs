using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using HB.FullStack.WebApi;
using HB.FullStack.WebApi.ApiKeyAuthentication;
using HB.FullStack.WebApi.Filters;
using HB.FullStack.WebApi.Security;
using HB.FullStack.WebApi.Startup;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;

using Polly;

using Serilog;

using StackExchange.Redis;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebApiServiceRegister
    {
        public static IServiceCollection AddFullStackWebApi(this IServiceCollection services,
            Action<DataProtectionSettings> dataProtectionSettingsAction,
            Action<JwtClientSettings> jwtClientSettingsAction,
            Action<ApiKeyAuthenticationOptions> apiKeyAuthenticationOptionsAction,
            Action<ForwardedHeadersOptions> forwardedHeaderOptionsAction,
            Action<InitializationOptions> initializationOptionsAction)
        {
            //DataProtection
            services.AddDataProtectionWithCertInRedis(dataProtectionSettingsAction);

            //Authentication
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtAuthentication(jwtClientSettingsAction, onChallenge: OnJwtChallengeAsync,
                    onTokenValidated: OnJwtTokenValidatedAsync,
                    onAuthenticationFailed: OnJwtAuthenticationFailedAsync,
                    onForbidden: OnJwtForbiddenAsync,
                    onMessageReceived: OnJwtMessageReceivedAsync)
                .AddApiKeyAuthentication(apiKeyAuthenticationOptionsAction);

            //Authorization
            services
                .AddAuthorization(options =>
                 {
                     options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                 });

            //Controller
            services.AddControllersWithConfiguration(addAuthentication: true);

            //Proxy
            services.Configure(forwardedHeaderOptionsAction);


            //HB.FullStack.WebApi Services
            services.AddSingleton<ISecurityService, DefaultSecurityService>();
            services.AddSingleton<ICommonResourceTokenService, CommonResourceTokenService>();
            services.AddScoped<UserActivityFilter>();
            services.AddScoped<CheckCommonResourceTokenFilter>();

            //Initialization
            services
                .Configure(initializationOptionsAction)
                .AddHostedService<InitializationHostedService>();


            return services;
        }

        public static IServiceCollection AddDataProtectionWithCertInRedis(this IServiceCollection services, Action<DataProtectionSettings> action)
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

        public static IServiceCollection AddControllersWithConfiguration(this IServiceCollection services, bool addAuthentication = true)
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
            //验证Body 中的DeviceId 与 JWT 中的DeviceId 是否一致
            string? jwtDeviceId = c.Principal?.GetDeviceId();

            string? requestDeviceId = c.HttpContext.Request.GetHeaderValueAs<string>(ClientNames.DEVICE_ID);

            requestDeviceId ??= c.HttpContext.Request.GetValue(ClientNames.DEVICE_ID);

            if (!string.IsNullOrWhiteSpace(jwtDeviceId) && jwtDeviceId.Equals(requestDeviceId, Globals.ComparisonIgnoreCase))
            {
                return Task.CompletedTask;
            }

            c.Fail("Token DeviceId do not match Request DeviceId");

            Log.Warning($"DeviceId:{requestDeviceId} do not match Request DeviceId : {jwtDeviceId}");

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