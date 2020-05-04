using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class EnvironmentUtil
    {
        public static bool IsDevelopment()
        {
            string? aspnetcore_environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (!aspnetcore_environment.IsNullOrEmpty())
            {
                return aspnetcore_environment!.Equals("Development", GlobalSettings.ComparisonIgnoreCase);
            }

            return false;
        }
    }
}
