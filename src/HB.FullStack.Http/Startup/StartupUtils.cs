﻿using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using HB.FullStack.Database;
using HB.FullStack.Identity;
using HB.FullStack.Lock.Distributed;
using HB.FullStack.WebApi;
using HB.FullStack.WebApi.Filters;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using Polly;

using Serilog;

using StackExchange.Redis;

namespace System
{
    public static class StartupUtils
    {
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

        /// <summary>
        /// audience:我是谁，即jwt是颁发给谁的
        /// authority:当局。我该去向谁核实，即是谁颁发了这个jwt
        /// </summary>
        public static AuthenticationBuilder AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration,
            Func<JwtBearerChallengeContext, Task> onChallenge,
            Func<TokenValidatedContext, Task> onTokenValidated,
            Func<AuthenticationFailedContext, Task> onAuthenticationFailed,
            Func<ForbiddenContext, Task> onForbidden,
            Func<MessageReceivedContext, Task> onMessageReceived)
        {
            JwtClientSettings jwtSettings = new JwtClientSettings();
            configuration.Bind(jwtSettings);

            X509Certificate2 encryptCert = CertificateUtil.GetCertificateFromSubjectOrFile(
                jwtSettings.JwtContentCertificateSubject,
                jwtSettings.JwtContentCertificateFileName,
                jwtSettings.JwtContentCertificateFilePassword);

            return
                services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(jwtOptions =>
                {
                    //#if DEBUG
                    //                    jwtOptions.RequireHttpsMetadata = false;
                    //#endif
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
                        OnChallenge = onChallenge,
                        OnAuthenticationFailed = onAuthenticationFailed,
                        OnMessageReceived = onMessageReceived,
                        OnTokenValidated = onTokenValidated,
                        OnForbidden = onForbidden
                    };

                    //#if DEBUG
                    //                    //这是为了ubuntu这货，在开发阶段不认开发证书。这个http请求，是由jwt audience 发向 jwt authority的。authority配置了正式证书后，就没问题了
                    //                    jwtOptions.BackchannelHttpHandler = new HttpClientHandler
                    //                    {
                    //                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                    //                        {
                    //                            if (cert!.Issuer.Equals("CN=localhost", GlobalSettings.Comparison))
                    //                                return true;
                    //                            return errors == System.Net.Security.SslPolicyErrors.None;
                    //                        }
                    //                    };
                    //#endif
                });
        }

        public static IServiceCollection AddControllersWithConfiguration(this IServiceCollection services, bool addAuthentication = true)
        {
            Assembly httpFrameworkAssembly = typeof(GlobalExceptionController).Assembly;

            //services.AddTransient<ProblemDetailsFactory, CustomProblemDetailsFactory>();

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

            return services;
        }

        /// <summary>
        /// 返回是否有Migration被执行
        /// </summary>
        /// <param name="database"></param>
        /// <param name="lockManager"></param>
        /// <param name="migrations"></param>
        /// <returns></returns>
        public static async Task<bool> InitializeDatabaseAsync(HB.FullStack.Database.IDatabase database, IDistributedLockManager lockManager, IEnumerable<Migration>? migrations)
        {
            GlobalSettings.Logger.LogDebug("开始初始化数据库:{DatabaseNames}", database.DatabaseNames.ToJoinedString(","));

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

                GlobalSettings.Logger.LogDebug("获取了初始化数据库的锁:{DatabaseNames}", database.DatabaseNames.ToJoinedString(","));

                return await database.InitializeAsync(migrations).ConfigureAwait(false);
            }
            finally
            {
                await distributedLock.DisposeAsync().ConfigureAwait(false);
            }
        }

        private static void ThrowIfDatabaseInitLockNotGet(IEnumerable<string> databaseNames)
        {
            throw WebApiExceptions.DatabaseInitLockError(databases: databaseNames);
        }
    }
}