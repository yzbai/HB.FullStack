using HB.Framework.Common;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Component.CentralizedLogger.LoggerServer
{
    public class LoggerEventHandler : IEventHandler
    {
        private LoggerEventHandlerOptions _options;
        private IElasticClient _elasticClient;
        private ILogger _logger;

        public LoggerEventHandler(IOptions<LoggerEventHandlerOptions> options, IElasticClient elasticClient, ILogger<LoggerEventHandler> logger)
        {
            _options = options.Value;
            _elasticClient = elasticClient;
            _logger = logger;

            //var _elasticSettings = new ConnectionSettings(new Uri(_options.ElasticSearchUri));
            //_elasticClient = new ElasticClient(_elasticSettings);

        }

        public EventHandlerConfig GetConfig()
        {
            return new EventHandlerConfig {
                ServerName = _options.ServerName,
                EventName = _options.LogEventName,
                SubscribeGroup = _options.SubscribeGroup
            };
        }

        public void Handle(string jsonString)
        {
            LogEntity logEntity = null;

            try
            {
                logEntity = DataConverter.FromJson<LogEntity>(jsonString);
            }
            catch(Exception ex)
            {
                logEntity = new LogEntity() { Exception = ex, Message = jsonString, DateTime = DateTime.Now };
            }

            if (logEntity == null)
            {
                logEntity = new LogEntity() { Message = jsonString, DateTime = DateTime.Now};
            }

            IIndexResponse indexResponse = _elasticClient.Index(logEntity, idx => idx.Index(_options.LogEventName.ToLower(GlobalSettings.Culture)));

            if (indexResponse != null)
            {
                _logger.LogDebug(indexResponse.DebugInformation);

                if (!indexResponse.IsValid)
                {
                    _logger.LogError(indexResponse.DebugInformation);
                }
            }

        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).

                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~LoggerEventHandler()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        
        #endregion
    }
}
