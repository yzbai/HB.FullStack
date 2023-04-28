using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Compact;

namespace HB.FullStack.Server.WebLib
{
    public static class SerilogHelper
    {
        private const string DefaultLogDirectory = "logs";
        private const string LogFileNameTemplate = "{0}_{1}_Id.log";
        private const string LogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}[{Level:u3}] {Message:lj} [{SourceContext}]{NewLine}{Exception}{Properties:j}{NewLine}";

        private static SerilogLoggerFactory? _loggerFactory;

        public static void CloseLogs()
        {
            _loggerFactory?.Dispose();
            Log.CloseAndFlush();
        }

        /// <summary>
        /// https://github.com/serilog/serilog-aspnetcore
        /// </summary>
        public static void OpenLogs(string? logDirectory = null)
        {
            LoggerConfiguration loggerConfiguration = CreateLoggerConfiguration(logDirectory);

            //TODO: 这句话要在前面?
            _loggerFactory = new SerilogLoggerFactory();

            Log.Logger = loggerConfiguration.CreateLogger();

            Log.Logger.Information("Serilog 创建成功");

            //设置全局Logger
            Globals.Logger = _loggerFactory.CreateLogger(nameof(Globals));

            //捕捉漏网Exception
            TaskScheduler.UnobservedTaskException += (sender, args) =>
            {
                Globals.Logger.LogCritical(args.Exception, "未被发现的Task异常，Sender : {Sender}", sender);
                args.SetObserved();
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception? ex = e.ExceptionObject as Exception;
                Globals.Logger.LogCritical(ex, "未被发现的Exception, {Sender}, {IsTerminating}", sender, e.IsTerminating);
            };
        }

        private static LoggerConfiguration CreateLoggerConfiguration(string? logDirectory)
        {
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration();

            if (EnvironmentUtil.IsDevelopment())
            {
                loggerConfiguration
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("MySqlConnector", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Debug)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning); // for use Serialog request log;
            }
            else if (EnvironmentUtil.IsStaging())
            {
                loggerConfiguration
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning); // for use Serialog request log;
            }
            else if (EnvironmentUtil.IsProduction())
            {
                loggerConfiguration
                    .MinimumLevel.Information()
                    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning); // for use Serialog request log;
            }

            string logFileName = string.Format(
                CultureInfo.InvariantCulture,
                LogFileNameTemplate,
                EnvironmentUtil.ApplicationName ?? "UnKown" + Guid.NewGuid().ToString(),
                EnvironmentUtil.MachineId.GetValueOrDefault());

            logDirectory ??= DefaultLogDirectory;

            string logFilePath = Path.Combine(logDirectory, logFileName);

            loggerConfiguration
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.File(
                    formatter: new RenderedCompactJsonFormatter(),
                    path: logFilePath,
                    rollingInterval: RollingInterval.Day);

            if (EnvironmentUtil.IsDevelopment())
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: LogOutputTemplate, formatProvider: null);
            }

            return loggerConfiguration;
        }
    }
}