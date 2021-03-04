using HB.FullStack.Common.Api;
using HB.FullStack.Database;
using HB.FullStack.Identity;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.Server;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Polly;

using Serilog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace System
{
    public static class StartupExtensions
    {
        /// <summary>
        /// AddDataProtectionWithCertInRedis
        /// </summary>
        /// <param name="services"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        /// <exception cref="ServerException"></exception>
        public static IServiceCollection AddDataProtectionWithCertInRedis(this IServiceCollection services, Action<DataProtectionSettings> action)
        {
            DataProtectionSettings dataProtectionSettings = new DataProtectionSettings();
            action(dataProtectionSettings);

            string redisKey = $"{dataProtectionSettings.ApplicationName}_{EnvironmentUtil.AspNetCoreEnvironment}_dpk";
            X509Certificate2? certificate2 = CertificateUtil.GetBySubject(dataProtectionSettings.CertificateSubject);

            if (certificate2 == null)
            {
                Log.Fatal($"Cert For DataProtection not found. CertSubject:{dataProtectionSettings.CertificateSubject}");
                throw new ServerException(ServerErrorCode.DataProtectionCertNotFound, $"Subject:{dataProtectionSettings.CertificateSubject}");
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
                        .SetApplicationName(dataProtectionSettings.ApplicationName)
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
        /// <exception cref="ServerException"></exception>
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            JwtClientSettings jwtSettings = new JwtClientSettings();
            configuration.Bind(jwtSettings);

            //TODO: 在appsettings.json中暂时用了DataProtection的证书，正式发布时需要换掉
            X509Certificate2? encryptCert = CertificateUtil.GetBySubject(jwtSettings.DecryptionCertificateSubject);

            if (encryptCert == null)
            {
                throw new ServerException(ServerErrorCode.JwtEncryptionCertNotFound, $"Subject:{jwtSettings.DecryptionCertificateSubject}");
            }

            return
                services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer((Action<JwtBearerOptions>)(jwtOptions =>
                {
                    jwtOptions.Audience = jwtSettings.Audience;
                    jwtOptions.Authority = jwtSettings.Authority;
                    jwtOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        RequireAudience = true,
                        TryAllIssuerSigningKeys = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        TokenDecryptionKey = CredentialHelper.GetSecurityKey(encryptCert)
                    };
                    jwtOptions.Events = new JwtBearerEvents
                    {
                        OnChallenge = c =>
                        {
                            c.HandleResponse();
                            c.Response.StatusCode = 401;

                            //if (c.Request.Path.StartsWithSegments("/api", GlobalSettings.ComparisonIgnoreCase))
                            //{
                            c.Response.ContentType = "application/problem+json";

                            ApiErrorCode error = c.AuthenticateFailure switch
                            {
                                null => ApiErrorCode.NoAuthority,
                                SecurityTokenExpiredException s => ApiErrorCode.AccessTokenExpired,
                                _ => ApiErrorCode.NoAuthority
                            };

                            ApiError errorResponse = new ApiError(error, $"Exception:{c.AuthenticateFailure?.Message}, Error:{c.Error}, ErrorDescription:{c.ErrorDescription}, ErrorUri:{c.ErrorUri}");

                            return c.Response.WriteAsync(SerializeUtil.ToJson(errorResponse));
                            //}
                            //else
                            //{
                            //    return Task.CompletedTask;
                            //}
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
                }));
        }

        public static IServiceCollection AddControllersWithConfiguration(this IServiceCollection services)
        {
            Assembly httpFrameworkAssembly = typeof(ExceptionController).Assembly;
            
            //services.AddTransient<ProblemDetailsFactory, CustomProblemDetailsFactory>();

            services
                .AddControllers(options =>
                {
                    //need authenticated by default. no need add [Authorize] everywhere
                    AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    options.Filters.Add(new AuthorizeFilter(policy));
                    //options.Filters
                })
                .AddJsonOptions(options =>
                {
                    SerializeUtil.Configure(options.JsonSerializerOptions);
                })
                .ConfigureApiBehaviorOptions((Action<ApiBehaviorOptions>)(apiBehaviorOptions =>
                {
                    apiBehaviorOptions.InvalidModelStateResponseFactory = actionContext =>
                    {
                        ApiError apiErrorResponse = new ApiError((ApiErrorCode)ApiErrorCode.ModelValidationError, actionContext.ModelState.GetErrors());

                        return new BadRequestObjectResult(apiErrorResponse)
                        {
                            ContentTypes = { "application/problem+json" }
                        };
                    };

                }))
                .PartManager.ApplicationParts.Add(new AssemblyPart(httpFrameworkAssembly));

            return services;
        }

        /// <summary>
        /// InitializeDatabaseAsync
        /// </summary>
        /// <param name="database"></param>
        /// <param name="lockManager"></param>
        /// <returns></returns>
        /// <exception cref="DatabaseException"></exception>
        public static async Task InitializeDatabaseAsync(HB.FullStack.Database.IDatabase database, IDistributedLockManager lockManager, IEnumerable<Migration>? migrations)
        {
            GlobalSettings.Logger.LogDebug($"开始初始化数据库:{database.DatabaseNames.ToJoinedString(",")}");

            IDistributedLock distributedLock = await lockManager.LockAsync(
                resources: database.DatabaseNames,
                expiryTime: TimeSpan.FromMinutes(5),
                waitTime: TimeSpan.FromMinutes(10)).ConfigureAwait(false);

            try
            {
                if (!distributedLock.IsAcquired)
                {
                    ThrowIfDatabaseInitLockNotGet(database.DatabaseNames);
                }

                GlobalSettings.Logger.LogDebug($"获取了初始化数据库的锁:{database.DatabaseNames.ToJoinedString(",")}");

                await database.InitializeAsync(migrations).ConfigureAwait(false);
            }
            finally
            {
                await distributedLock.DisposeAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// ThrowIfDatabaseInitLockNotGet
        /// </summary>
        /// <param name="databaseNames"></param>
        /// <exception cref="DatabaseException"></exception>
        private static void ThrowIfDatabaseInitLockNotGet(IEnumerable<string> databaseNames)
        {
            throw new DatabaseException(DatabaseErrorCode.DatabaseInitLockError, $"Database:{databaseNames.ToJoinedString(",")}");
        }
    }
}
