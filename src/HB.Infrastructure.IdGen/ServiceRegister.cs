using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using HB.Infrastructure.IdGen;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterIdGenExtensions
    {
        public static IServiceCollection AddIdGen(this IServiceCollection services, IConfiguration configuration)
        {
            IdGenSettings settings = new IdGenSettings();
            configuration.Bind(settings);

            FlackIdGen.Initialize(settings);
            return services;
        }

        public static IServiceCollection AddIdGen(this IServiceCollection services, Action<IdGenSettings> action)
        {
            IdGenSettings settings = new IdGenSettings();
            action(settings);

            FlackIdGen.Initialize(settings);
            return services;
        }
    }
}
