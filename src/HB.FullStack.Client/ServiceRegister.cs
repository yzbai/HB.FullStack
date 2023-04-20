/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Client.Base;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ClientServiceRegister
    {
        public static IServiceCollection AddFullStackClient(this IServiceCollection services)
        {
            //Base
            services.AddSingleton<IClientModelSettingFactory, ClientModelSettingFactory>();

            return services;
        }
    }
}