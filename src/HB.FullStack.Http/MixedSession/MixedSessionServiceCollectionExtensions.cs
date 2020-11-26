// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding session services to the DI container.
    /// </summary>
    public static class MixedSessionServiceCollectionExtensions
    {
        public static IServiceCollection AddMixedSession(this IServiceCollection services, Action<MixedSessionOptions> configure)
        {
            services.AddOptions();

            services.Configure(configure);

            return services.AddSession();
        }

        public static IServiceCollection AddMixedSession(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions();
            services.Configure<MixedSessionOptions>(configuration);
            return services;
        }


    }
}