using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using HB.FullStack.WebApi;

using Microsoft.Extensions.Logging;

namespace System
{
    public static class EnvironmentUtil
    {
        private const string HB_FULLSTACK_MACHINE_ID = "HB_FULLSTACK_MACHINE_ID";
        private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

        public static string? AspNetCoreEnvironment
        {
            get
            {
                return Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT);
            }

            set
            {
                Environment.SetEnvironmentVariable(ASPNETCORE_ENVIRONMENT, value);
            }
        }

        public static int? MachineId
        {
            get
            {
                string? str = Environment.GetEnvironmentVariable(HB_FULLSTACK_MACHINE_ID);

                return str.IsNullOrEmpty() ? null : Convert.ToInt32(str, CultureInfo.InvariantCulture);
            }
            set
            {
                Environment.SetEnvironmentVariable(HB_FULLSTACK_MACHINE_ID, value.ToString());
            }
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

        public static bool IsDevelopment()
        {
            return "Development".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }

        public static bool IsStaging()
        {
            return "Staging".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }

        public static bool IsProduction()
        {
            return "Production".Equals(AspNetCoreEnvironment, StringComparison.Ordinal);
        }

        public static void EnsureEnvironment()
        {
            //检查环境变量
            if (!IsAspnetcoreEnvironmentOk())
            {
                throw WebApiExceptions.StartupError(value: EnvironmentUtil.AspNetCoreEnvironment, cause: "环境变量ASPNETCORE_ENVIRONMENT设置错误");
            }

            if (MachineId.GetValueOrDefault() == 0)
            {
                throw WebApiExceptions.StartupError(value: EnvironmentUtil.MachineId, cause: "环境变量MachineId设置错误");
            }
        }
    }
}