using HB.FullStack.Server.Identity;
using HB.FullStack.Server.WebLib.ApiKeyAuthentication;
using HB.FullStack.Server.WebLib.Startup;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder authenticationBuilder, Action<ApiKeyAuthenticationOptions> configApiKeyAuthenticationOptions)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(ApiKeyAuthenticationOptions.DefaultScheme, configApiKeyAuthenticationOptions);
        }
        public static AuthenticationBuilder AddJwtAuthentication(this AuthenticationBuilder authenticationBuilder,
            Action<JwtClientSettings> configureJwtClientSettings,
            Func<JwtBearerChallengeContext, Task> onChallenge,
            Func<TokenValidatedContext, Task> onTokenValidated,
            Func<AuthenticationFailedContext, Task> onAuthenticationFailed,
            Func<ForbiddenContext, Task> onForbidden,
            Func<MessageReceivedContext, Task> onMessageReceived)
        {
            JwtClientSettings jwtSettings = new JwtClientSettings();
            configureJwtClientSettings(jwtSettings);

            X509Certificate2 encryptCert = CertificateUtil.GetCertificateFromSubjectOrFile(
                            jwtSettings.JwtContentCertificateSubject,
                            jwtSettings.JwtContentCertificateFileName,
                            jwtSettings.JwtContentCertificateFilePassword);

            return authenticationBuilder.AddJwtBearer(jwtOptions =>
            {
                //#if DEBUG
                //                    jwtOptions.RequireHttpsMetadata = false;
                //#endif
                jwtOptions.Audience = jwtSettings.Audience;

                //从Authority获取signing证书 公钥
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

                    //内容证书：jwt中签名一个证书，内容加密一个证书
                    //TODO: 是否也可以从Authority获取？
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
    }
}
