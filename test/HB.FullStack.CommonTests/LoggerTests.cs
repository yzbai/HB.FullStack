using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Serilog;
using Serilog.Exceptions;

namespace HB.FullStack.CommonTests
{
    public class LoggerTestItem
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);

        public string Comment { get; set; } = SecurityUtil.CreateRandomNumbericString(4);
    }

    [TestClass]
    public class LoggerTests
    {
        private readonly ServiceProvider _serviceProvider;

        public LoggerTests()
        {
            string LogOutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}[{Level:u3}] {Message:lj} [{SourceContext}]{NewLine}{Exception}{Properties:j}{NewLine}";
            //string template = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception} {Properties:j}{NewLine}";
            Log.Logger = new LoggerConfiguration()
                          .Enrich.FromLogContext()
                          .Enrich.WithExceptionDetails()
                          .WriteTo.Console(outputTemplate: LogOutputTemplate)
                          .CreateLogger();

            ServiceCollection services = new ServiceCollection();

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddSerilog();
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Serilog可以结构化Exception.Data内的数据，并且借助Enrich.WithExceptionDetails。可以完美记录Exception
        /// </summary>
        [TestMethod]
        public void Logger_Test_Structure_Json()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

            LoggerTestItem item = new LoggerTestItem();

            object obj = new { Name = "sfasdf", Age = 12 };

            object? nullObj = null;

            logger.LogWarning("This is a class : {@Item}", item);
            logger.LogWarning("This is a class : {@DynamicItem}", obj);
            logger.LogWarning("This is a class : {@NullItem}", nullObj);

            try
            {
                Exception ex1 = new Exception("This is a Test");
                ex1.Data["Context"] = obj;
                ex1.Data["Other"] = nullObj;
                ex1.Data["Item"] = item;

                Exception ex2 = new Exception("Outterrrrrrrrrrrrrrrrrr Exception", ex1);

                ex2.Data["TestMessage"] = new { XXX = "gaasfasdf", YYY = "asfasfasf" };

                throw ex2;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception Captured");
            }
        }

        public void tT()
        {

        }
    }
}
