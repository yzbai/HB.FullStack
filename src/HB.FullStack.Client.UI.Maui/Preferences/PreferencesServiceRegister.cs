using HB.FullStack.Client.UI.Maui;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PreferencesServiceRegister
    {
        public static IServiceCollection AddPreferences(this IServiceCollection services)
        {
            services.AddSingleton<IPreferenceProvider, MauiPreferenceProvider>();
            return services;
        }
    }
}
