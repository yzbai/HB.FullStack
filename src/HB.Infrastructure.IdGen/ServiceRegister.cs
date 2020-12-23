using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using HB.Infrastructure.IdGen;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceRegisterIdGenExtensions
    {
        public static IServiceCollection AddIdGen(this IServiceCollection services, string machineId)
        {
            IdGenDistributedId.Initialize(Convert.ToInt32(machineId));
            return services;
        }
    }
}
