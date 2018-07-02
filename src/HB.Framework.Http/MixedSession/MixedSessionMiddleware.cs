// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.Session
{
    /// <summary>
    /// Enables the session state for the application.
    /// </summary>
    public class MixedSessionMiddleware
    {
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();
        private const int SessionKeyLength = 36; // "382c74c3-721d-4f34-80e5-57657b6cbc27"
        private static readonly Func<bool> ReturnTrue = () => true;
        private readonly RequestDelegate _next;
        private readonly MixedSessionOptions _options;
        private readonly ILogger _logger;
        private readonly ISessionStore _sessionStore;
        private readonly IDataProtector _dataProtector;

        /// <summary>
        /// Creates a new <see cref="SessionMiddleware"/>.
        /// </summary>
        /// <param name="next">The <see cref="RequestDelegate"/> representing the next middleware in the pipeline.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> representing the factory that used to create logger instances.</param>
        /// <param name="sessionStore">The <see cref="ISessionStore"/> representing the session store.</param>
        /// <param name="options">The session configuration options.</param>
        public MixedSessionMiddleware(
             RequestDelegate next,
             ILoggerFactory loggerFactory,
             IDataProtectionProvider dataProtectionProvider,
             ISessionStore sessionStore,
             IOptions<MixedSessionOptions> options)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            if (dataProtectionProvider == null)
            {
                throw new ArgumentNullException(nameof(dataProtectionProvider));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _next = next;
            _logger = loggerFactory.CreateLogger<MixedSessionMiddleware>();
            _options = options.Value;
            _sessionStore = sessionStore;
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(MixedSessionMiddleware));
        }

        /// <summary>
        /// Invokes the logic of the middleware.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>A <see cref="Task"/> that completes when the middleware has completed processing.</returns>
        public async Task Invoke(HttpContext context)
        {
            Func<bool> tryEstablishSession = ReturnTrue;
            bool isNewSessionKey = false;

            string sessionKey = GetSessionKey(context);

            if (isSessionKeyInValid(sessionKey))
            {
                byte[] guidBytes = new byte[16];
                CryptoRandom.GetBytes(guidBytes);
                sessionKey = new Guid(guidBytes).ToString();

                var establisher = new SessionEstablisher(context, sessionKey, _options);
                tryEstablishSession = establisher.TryEstablishSession;
                isNewSessionKey = true;
            }

            var feature = new SessionFeature();
            feature.Session = _sessionStore.Create(sessionKey, _options.IdleTimeout, _options.IOTimeout, tryEstablishSession, isNewSessionKey);
            context.Features.Set<ISessionFeature>(feature);

            try
            {
                await _next(context);
            }
            finally
            {
                context.Features.Set<ISessionFeature>(null);

                if (feature.Session != null)
                {
                    try
                    {
                        await feature.Session.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error closing the session.", ex);
                    }
                }
            }
        }

        //TODO: add data protection later
        private string GetSessionKey(HttpContext context)
        {
            string sessionKey = context.GetValueFromRequest(_options.Name, includeCookie: false);

            if (!string.IsNullOrWhiteSpace(sessionKey))
            {
                return sessionKey;
            }

            var cookieValue = context.Request.Cookies[_options.Name];

            return cookieValue;
        }

        private bool isSessionKeyInValid(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return true;
            }

            if (key.Length != SessionKeyLength)
            {
                return true;
            }

            //TODO:还可以加入更多的格式验证

            return false;
        }

        private class SessionEstablisher
        {
            private readonly HttpContext _context;
            private readonly string _cookieValue;
            private readonly MixedSessionOptions _options;
            private bool _shouldEstablishSession;

            public SessionEstablisher(HttpContext context, string cookieValue, MixedSessionOptions options)
            {
                _context = context;
                _cookieValue = cookieValue;
                _options = options;
                context.Response.OnStarting(OnStartingCallback, state: this);
            }

            private static Task OnStartingCallback(object state)
            {
                var establisher = (SessionEstablisher)state;
                if (establisher._shouldEstablishSession)
                {
                    establisher.SetCookie();
                }
                return Task.FromResult(0);
            }

            private void SetCookie()
            {
                var cookieOptions = new CookieOptions
                {
                    SameSite = _options.SameSiteMode,
                    Secure = _options.CookieSecure == CookieSecurePolicy.Always || (_options.CookieSecure == CookieSecurePolicy.SameAsRequest && _context.Request.IsHttps),
                    Domain = _options.CookieDomain,
                    HttpOnly = _options.CookieHttpOnly,
                    Path = _options.CookiePath ?? "/",
                    Expires = DateTimeOffset.Now.Add(_options.IdleTimeout)
                };

                _context.Response.Cookies.Append(_options.Name, _cookieValue, cookieOptions);

                _context.Response.Headers["Cache-Control"] = "no-cache";
                _context.Response.Headers["Pragma"] = "no-cache";
                _context.Response.Headers["Expires"] = "-1";
            }

            // Returns true if the session has already been established, or if it still can be because the response has not been sent.
            internal bool TryEstablishSession()
            {
                return (_shouldEstablishSession |= !_context.Response.HasStarted);
            }
        }
    }
}