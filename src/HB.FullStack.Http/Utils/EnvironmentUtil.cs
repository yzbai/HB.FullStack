using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Microsoft.Extensions.Logging;

namespace System
{
    public static class EnvironmentUtil
    {
        private const string HB_FULLSTACK_MACHINE_ID = "HB_FULLSTACK_MACHINE_ID";

        private static string? _aspnetcore_environment;

        public static string? AspNetCoreEnvironment
        {
            get
            {
                if (_aspnetcore_environment.IsNullOrEmpty())
                {
                    _aspnetcore_environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                }

                return _aspnetcore_environment;
            }
        }

        public static int? MachineId
        {
            get
            {
                string? str = Environment.GetEnvironmentVariable(HB_FULLSTACK_MACHINE_ID);

                if (str.IsNullOrEmpty())
                {
                    return null;
                }

                return Convert.ToInt32(str, CultureInfo.InvariantCulture);
            }
        }

        public static bool IsDevelopment()
        {
            return "Development".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }

        public static bool IsAspnetcoreEnvironmentOk()
        {
            return AspNetCoreEnvironment switch
            {
                "Development" => true,
                "Staging" => true,
                "Production" => true,
                _ => false
            };
        }

        public static bool IsStaging()
        {
            return "Staging".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }

        public static bool IsProduction()
        {
            return "Production".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }
    }
}
