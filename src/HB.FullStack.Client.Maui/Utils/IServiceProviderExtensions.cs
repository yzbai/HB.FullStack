using Microsoft.Maui.Controls;

namespace System
{
    public static class IServiceProviderExtensions
    {
        public static Page? GetPage(this IServiceProvider services, string pageFullName)
        {
            Type? pageType = Type.GetType(pageFullName);

            if (pageType == null)
            {
                return null;
            }

            return (Page?)services.GetService(pageType);
        }
    }
}
