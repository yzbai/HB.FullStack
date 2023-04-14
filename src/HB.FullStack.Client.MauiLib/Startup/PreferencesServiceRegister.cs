using Microsoft.Extensions.DependencyInjection;

using System;

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
