using System;

using Microsoft.AspNetCore.Builder;

namespace HB.FullStack.WebApi
{
    public static class GlobalWebApplicationAccessor
    {
        private static WebApplication? _application;

        public static WebApplication Application
        {
            get
            {
                if (_application == null)
                {
                    throw WebApiExceptions.ShouldSetGlobalWebApplicationAccessorAtBegining();
                }

                return _application;
            }
            set => _application = value;
        }

        public static IServiceProvider Services => Application.Services;
    }
}
