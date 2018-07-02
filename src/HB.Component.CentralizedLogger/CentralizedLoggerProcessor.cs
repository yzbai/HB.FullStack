using HB.Framework.Common;
using HB.Framework.EventBus;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Component.CentralizedLogger
{
    public class CentralizedLoggerProcessor : IDisposable
    {
        private const int _maxQueuedMessages = 2048;

        private readonly BlockingCollection<CentralizedLogEntity> _messageQueue = new BlockingCollection<CentralizedLogEntity>(_maxQueuedMessages);
        private readonly Task _outputTask;

        private IEventBus _eventBus;
        private CentralizedLoggerOptions _options;

        public CentralizedLoggerProcessor(IEventBus eventBus, IOptions<CentralizedLoggerOptions> options)
        {
            _eventBus = eventBus;
            _options = options.Value;

            _outputTask = Task.Factory.StartNew(()=> {

                foreach (var message in _messageQueue.GetConsumingEnumerable())
                {
                    PublishLog(message);
                }

            }, TaskCreationOptions.LongRunning);
        }

        private void PublishLog(CentralizedLogEntity entity)
        {
            _eventBus.Publish(_options.LogEventName, DataConverter.ToJson(entity));
        }

        public void EnqueLogEntity(CentralizedLogEntity logEntity)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(logEntity);
                    return;
                }
                catch (InvalidOperationException ex)
                {
                    throw ex;
                }
            }

            //Adding is completed so just log the message
            PublishLog(logEntity);
        }

        #region Dispose Pattern

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free manage
                _messageQueue.CompleteAdding();

                try
                {
                    _outputTask.Wait(2000);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }

            // Free unmanaged

            _disposed = true;
        }

        ~CentralizedLoggerProcessor()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
