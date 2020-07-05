using HB.Framework.Common.Api;
using HB.Framework.Http;
using HB.Framework.Http.Startup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace System
{
    public static class StartupUtil
    {
        public static IServiceCollection AddDataProtectionWithCertInRedis(this IServiceCollection services, Action<DataProtectionSettings> action)
        {
            DataProtectionSettings dataProtectionSettings = new DataProtectionSettings();
            action(dataProtectionSettings);

            string redisKey = $"{dataProtectionSettings.ApplicationDiscriminator}_{EnvironmentUtil.AspNetCoreEnvironment}_dpk";
            X509Certificate2? certificate2 = CertificateUtil.GetBySubject(dataProtectionSettings.CertificateSubject);

            if (certificate2 == null)
            {
                Log.Fatal($"Cert For DataProtection not found. CertSubject:{dataProtectionSettings.CertificateSubject}");
                throw new Exception($"DataProtection Certificate Not Found. CertSubject:{dataProtectionSettings.CertificateSubject}");
            }

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
                        .SetApplicationName(dataProtectionSettings.ApplicationDiscriminator)
                        .ProtectKeysWithCertificate(certificate2)
                        .PersistKeysToStackExchangeRedis(redisMultiplexer, redisKey);
                });

            return services;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="audience">我是谁，即jwt是颁发给谁的</param>
        /// <param name="authority">当局。我该去向谁核实，即是谁颁发了这个jwt</param>
        /// <returns></returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, Action<JwtSettings> action)
        {
            JwtSettings jwtSettings = new JwtSettings();
            action(jwtSettings);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(jwtOptions =>
            {
                jwtOptions.Audience = jwtSettings.Audience;
                jwtOptions.Authority = jwtSettings.Authority;
                jwtOptions.Events = new JwtBearerEvents
                {
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = 401;

                        if (c.Request.Path.StartsWithSegments("/api", GlobalSettings.ComparisonIgnoreCase))
                        {
                            c.Response.ContentType = "application/json";

                            ApiError error = c.AuthenticateFailure switch
                            {
                                SecurityTokenExpiredException s => ApiError.ApiTokenExpired,
                                _ => ApiError.NOAUTHORITY
                            };

                            ApiErrorResponse errorResponse = new ApiErrorResponse(error, c.AuthenticateFailure.Message);

                            return c.Response.WriteAsync(SerializeUtil.ToJson(errorResponse));
                        }
                        else
                        {
                            return Task.CompletedTask;
                        }
                    },
                    OnAuthenticationFailed = c =>
                    {
                        //TODO: 说明这个AccessToken有风险，应该拒绝他的刷新。Black相应的RefreshToken
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = c =>
                    {
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = c =>
                    {
                        //验证DeviceId 与 JWT 中的DeviceId 是否一致
                        string? jwt_DeviceId = c.Principal?.GetDeviceId();
                        string request_DeviceId = c.HttpContext.Request.GetValue(ClientNames.DeviceId);


                        if (!string.IsNullOrWhiteSpace(jwt_DeviceId) && jwt_DeviceId.Equals(request_DeviceId, GlobalSettings.ComparisonIgnoreCase))
                        {
                            return Task.CompletedTask;
                        }

                        c.Fail("Token DeviceId do not match Request DeviceId");

                        Log.Warning($"DeviceId:{request_DeviceId} do not match Request DeviceId : {jwt_DeviceId}");

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddControllersWithConfiguration(this IServiceCollection services)
        {
            Assembly httpFrameworkAssembly = typeof(ExceptionController).Assembly;

            services
                .AddControllers(options =>
                {
                    //need authenticated by default. no need add [Authorize] everywhere
                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                })
                .ConfigureApiBehaviorOptions(apiBehaviorOptions =>
                {
                    apiBehaviorOptions.InvalidModelStateResponseFactory = actionContext =>
                    {
                        ApiErrorResponse apiErrorResponse = new ApiErrorResponse(ApiError.MODELVALIDATIONERROR, actionContext.ModelState.GetErrors());

                        return new BadRequestObjectResult(apiErrorResponse)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };

                })
                .PartManager.ApplicationParts.Add(new AssemblyPart(httpFrameworkAssembly));

            return services;
        }
    }
}
