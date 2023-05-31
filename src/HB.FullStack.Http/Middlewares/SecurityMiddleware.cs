using System;
using System.Threading.Tasks;

using HB.FullStack.Server.Identity;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Extensions;

namespace HB.FullStack.Server.WebLib.Middlewares
{
    public class SecurityMiddleware<TId> : IMiddleware
    {
        private readonly ILogger<SecurityMiddleware<TId>> _logger;
        private readonly IIdentityService<TId> _identityService;

        public SecurityMiddleware(ILogger<SecurityMiddleware<TId>> logger, IIdentityService<TId> identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                string? ip = context.GetIpAddress();
                string? url = context.Request?.GetDisplayUrl();
                string? httpMethod = context.Request?.Method;

                CheckIpRate(ip);

                //TODO: IP 白名单， 用于服务器之间的沟通

                //TODO: clientId & IP , clientID & 地理位置

                //TODO: SecurityCheck Here

                string? clientId = context.GetClientId();
                string? clientVersion = context.GetClientVersion();
                string? timestamp = context.GetTimestamp();

                CheckClientIdAndVersion(ip, clientId, clientVersion);

                var principal = context.User;

                TId? tokenCredentialId = principal != null ? principal.GetTokenCredentialId<TId>() : default;
                TId? userId = principal != null ? principal.GetUserId<TId>() : default;

                await next(context);

                
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(ex, "SecurityMiddleware 中捕捉到错误，不应该。请检查,是否有漏捕捉。{RequestUrl}", context.Request?.GetDisplayUrl());
                await OnErrorAsync(context);
            }
        }

        private void CheckIpRate(string? ip)
        {

        }

        private void CheckClientIdAndVersion(string? ip, string? clientId, string? clientVersion)
        {
            if (clientVersion.IsNullOrEmpty() || clientId.IsNullOrEmpty())
            {
                //TODO: 完善这个
                //throw WebExceptions.SecurityCheck();
            }

            //TODO: 过滤适应的clientversion
        }

        private static async Task OnErrorAsync(HttpContext context)
        {
            context.Response.StatusCode = 401;

            await context.Response.WriteAsJsonAsync(ErrorCodes.SecurityCheck);

        }
    }
}
