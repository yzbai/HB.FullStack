using System;
using System.Linq;
using HB.Framework.Common.Utility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace HB.Framework.Http
{

    public class ConfigurationMonitor : IConfigurationMonitor
    {
        private byte[] _appsettingsHash = new byte[20];
        private byte[] _appsettingsEnvHash = new byte[20];
        private readonly IHostingEnvironment _env;

        private IApplicationLifetime _applicationLifeTime;
        private readonly ILogger _logger;

 
        public ConfigurationMonitor(IConfiguration config, IHostingEnvironment env, IApplicationLifetime applicationLifetime, ILogger<ConfigurationMonitor> logger)
        {
            _env = env;
            _applicationLifeTime = applicationLifetime;
            _logger = logger;

            ChangeToken.OnChange<IConfigurationMonitor>(
                () => config.GetReloadToken(),
                InvokeChanged,
                this);
        }

        public bool MonitoringEnabled { get; set; } = false;
 
 
        private void InvokeChanged(IConfigurationMonitor state)
        {
            if (MonitoringEnabled)
            {
                byte[] appsettingsHash = FileHelper.ComputeHash("appSettings.json");
                byte[] appsettingsEnvHash = FileHelper.ComputeHash($"appSettings.{_env.EnvironmentName}.json");

                if (!_appsettingsHash.SequenceEqual(appsettingsHash) || 
                    !_appsettingsEnvHash.SequenceEqual(appsettingsEnvHash))
                {
                    string message = $"State updated at {DateTime.Now}";
                  

                    _appsettingsHash = appsettingsHash;
                    _appsettingsEnvHash = appsettingsEnvHash;

                    LogHelper.GlobalLogger.LogInformation("appsettings.json 配置文件变动");
                    LogHelper.GlobalLogger.LogInformation("系统即将重启......");


                    //TODO: 这里放关闭应用前，需要做的工作

                    _applicationLifeTime.StopApplication();
                }
            }
        }
 
    }
}