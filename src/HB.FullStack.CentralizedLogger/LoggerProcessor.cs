using HB.Framework.Common;
using HB.Framework.EventBus;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Component.CentralizedLogger
{
    public class LoggerProcessor : IDisposable
    {
        private const int _maxQueuedMessages = 2048;

        private readonly BlockingCollection<LogEntity> _messageQueue = new BlockingCollection<LogEntity>(_maxQueuedMessages);
        private readonly Task _outputTask;

        private CancellationTokenSource _cts;
        private IEventBus _eventBus;
        private LoggerOptions _options;

        public LoggerProcessor(IEventBus eventBus, IOptions<LoggerOptions> options)
        {
            _eventBus = eventBus;
            _options = options.Value;
            _cts = new CancellationTokenSource();

            _outputTask = Task.Factory.StartNew(()=> {

                foreach (var message in _messageQueue.GetConsumingEnumerable())
                {
                    PublishLog(message);
                }

            }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        private void PublishLog(LogEntity entity)
        {
            _eventBus.Publish(_options.LogEventName, DataConverter.ToJson(entity));
        }

        public void EnqueLogEntity(LogEntity logEntity)
        {
            if (!_messageQueue.IsAddingCompleted)
            {
                try
                {
                    _messageQueue.Add(logEntity);
                    return;
                }
                catch (InvalidOperationException)
                {
                    //throw ex;
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
                catch (Exception /*ex*/)
                {
                    //throw ex;
                }

            }

            // Free unmanaged

            _disposed = true;
        }

        ~LoggerProcessor()
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
