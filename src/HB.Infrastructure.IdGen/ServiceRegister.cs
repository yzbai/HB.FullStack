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

            //使用环境变量覆盖
            if (EnvironmentUtil.MachineId.HasValue)
            {
                settings.MachineId = EnvironmentUtil.MachineId.Value;
            }

            FlackIdGen.Initialize(settings);
            return services;
        }

        public static IServiceCollection AddIdGen(this IServiceCollection services, Action<IdGenSettings> action)
        {
            IdGenSettings settings = new IdGenSettings();
            action(settings);

            //使用环境变量覆盖
            if (EnvironmentUtil.MachineId.HasValue)
            {
                settings.MachineId = EnvironmentUtil.MachineId.Value;
            }

            FlackIdGen.Initialize(settings);
            return services;
        }
    }
}
