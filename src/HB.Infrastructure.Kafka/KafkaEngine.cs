using HB.Framework.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.AspNetCore.Hosting;
using Confluent.Kafka.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace HB.Infrastructure.Kafka
{
    public class KafkaEngine : IEventBusEngine
    {
        private object _objLocker = new object();
        private KafkaEngineOptions _options;
        private ILogger _logger;

        private ConcurrentBag<CancellationTokenSource> _cancellationTokenSources;

        private ConcurrentDictionary<string, Producer<Null, string>> _producers;

        public KafkaEngine(IApplicationLifetime applicationLifetime, IOptions<KafkaEngineOptions> options, ILogger<KafkaEngine> logger)
        {
            _options = options.Value;
            _logger = logger;
            _producers = new ConcurrentDictionary<string, Producer<Null, string>>();

            applicationLifetime.ApplicationStopped.Register(() => {
                Dispose();
            });

            _cancellationTokenSources = new ConcurrentBag<CancellationTokenSource>();
        }

        public Task<PublishResult> PublishString(string serverName, string eventName, string data)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentNullException("serverName");
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException("eventName");
            }

            var producer = getProducerByServerName(serverName);

            var task = producer.ProduceAsync(eventName, null, data);

            return task.ContinueWith<PublishResult>(t =>
            {
                return t.Result.Error.HasError ? PublishResult.Fail(t.Result.Error.Reason) : PublishResult.Succeeded();
            });
        }

        public Task SubscribeAndConsume(string serverName, string subscriberGroup, string eventName, IEventHandler handler)
        {
            if (string.IsNullOrWhiteSpace(serverName))
            {
                throw new ArgumentNullException("serverName");
            }

            if (string.IsNullOrWhiteSpace(subscriberGroup))
            {
                throw new ArgumentNullException("subscriberGroup");
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                throw new ArgumentNullException("eventName");
            }

            if (handler == null)
            {
                throw new ArgumentNullException("action");
            }

            CancellationTokenSource cts = new CancellationTokenSource();

            _cancellationTokenSources.Add(cts);

            return Task.Factory.StartNew(() => {

                using (Consumer<Null, string> consumer = new Consumer<Null, string>(getConsumerConfig(serverName, subscriberGroup), null, new StringDeserializer(Encoding.UTF8)))
                {
                    consumer.OnMessage += (sender, message) => {
                        handler.Handle(message.Value);
                    };

                    consumer.OnError += (sender, error) => {
                        _logger.LogCritical(error.Reason);
                    };

                    consumer.Subscribe(eventName);

                    while (true)
                    {
                        consumer.Poll(TimeSpan.FromSeconds(_options.ConsumerPollSeconds));
                        consumer.CommitAsync();
                    }
                }

            }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        }

        private Producer<Null, string> getProducerByServerName(string serverName)
        {
            return _producers.GetOrAdd(serverName, sname => createProducer(sname));
        }

        private Producer<Null, string> createProducer(string serverName)
        {
            var producer = new Producer<Null, string>(getProducerConfig(serverName), null, new StringSerializer(Encoding.UTF8));

            producer.OnError += (obj, error) =>
            {
                _logger.LogCritical(error.Reason);
            };

            return producer;
        }

        private IDictionary<string, object> getProducerConfig(string serverName)
        {
            var serverInfo = _options.GetServerInfo(serverName);
            var producerConfig = serverInfo?.ProducerConfig?.CloningWithValues();

            if (producerConfig == null)
            {
                producerConfig = new Dictionary<string, string>();
            }

            if (!producerConfig.ContainsKey("bootstrap.servers"))
            {
                producerConfig["bootstrap.servers"] = serverInfo.Host;
            }

            return producerConfig.ConvertValue<string, string, object>(str => str);
        }

        private IDictionary<string, object> getConsumerConfig(string serverName, string subscriberGroup)
        {
            var serverInfo = _options.GetServerInfo(serverName);
            var consumerConfig = serverInfo?.ConsumerConfig?.CloningWithValues();

            if (consumerConfig == null)
            {
                consumerConfig = new Dictionary<string, string>();
            }

            if (!consumerConfig.ContainsKey("bootstrap.servers"))
            {
                consumerConfig["bootstrap.servers"] = serverInfo.Host;
            }

            if (!consumerConfig.ContainsKey("group.id"))
            {
                consumerConfig["group.id"] = subscriberGroup;
            }

            return consumerConfig.ConvertValue<string, string, object>(str => str);
        }

        #region Dispose Pattern

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // Free managed
                foreach (CancellationTokenSource cts in _cancellationTokenSources.ToArray())
                {
                    cts.Cancel();
                    try
                    {
                        Task.CompletedTask.Wait(TimeSpan.FromSeconds(_options.ConsumerCancellWaitSeconds));
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            // Free unmanaged

            if (_producers != null)
            {
                foreach (var item in _producers)
                {
                    item.Value?.Flush(TimeSpan.FromSeconds(_options.ProducerFlushWaitSeconds));
                    item.Value?.Dispose();
                }
            }

            _disposed = true;
        }

        ~KafkaEngine()
        {
            Dispose(false);
        }

        #endregion
    }
}