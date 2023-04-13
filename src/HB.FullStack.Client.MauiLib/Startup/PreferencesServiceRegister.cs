using HB.FullStack.Client.Maui;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.MauiLib.Startup
{
    public static class PreferencesServiceRegister
    {
        public static IServiceCollection AddPreferences(this IServiceCollection services)
        {
            services.AddSingleton<IPreferenceProvider, PreferenceProvider>();
            return services;
        }
    }
}
