using HB.Component.CentralizedLogger;
using Microsoft.Extensions.Logging;


namespace Microsoft.Extensions.Logging
{
    public static class LoggingBuilderExtensions
    {
        public static ILoggerFactory AddCentralizedLog(this ILoggerFactory factory, LoggerProvider loggerProvider)
        {
            factory.AddProvider(loggerProvider);
            return factory;
        }
    }
}
