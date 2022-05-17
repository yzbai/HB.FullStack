using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Client.UI.Maui
{
    public static class ServicesProviderUtil
    {
        public static IServiceProvider Current => IPlatformApplication.Current!.Services;

        public static TService? GetService<TService>() => Current.GetService<TService>();

        public static Page? GetPage(string pageFullName)
        {
            Type? pageType = Type.GetType(pageFullName);

            if (pageType == null)
            {
                return null;
            }

            return (Page?)Current.GetService(pageType);
        }
    }
}
