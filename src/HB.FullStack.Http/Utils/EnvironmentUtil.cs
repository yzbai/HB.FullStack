using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class EnvironmentUtil
    {
        private static string? _aspnetcore_environment;

        public static string AspNetCoreEnvironment
        {
            get
            {
                if (_aspnetcore_environment.IsNullOrEmpty())
                {
                    _aspnetcore_environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                }

                return _aspnetcore_environment!;
            }
        }

        public static bool IsDevelopment()
        {
            return AspNetCoreEnvironment.Equals("Development", GlobalSettings.ComparisonIgnoreCase);
        }
    }
}
