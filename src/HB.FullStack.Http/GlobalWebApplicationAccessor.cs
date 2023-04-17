using System;

using Microsoft.AspNetCore.Builder;

namespace HB.FullStack.Server.WebLib
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
                    throw WebExceptions.ShouldSetGlobalWebApplicationAccessorAtBegining();
                }

                return _application;
            }
            set => _application = value;
        }

        public static IServiceProvider Services => Application.Services;
    }
}
