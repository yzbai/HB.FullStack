using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.EventBus;
using HB.Framework.EventBus.Abstractions;
using HB.Framework.KVStore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HB.Infrastructure.RabbitMQ
{
    public class RabbitMQEventBusEngine : IEventBusEngine
    {
        private ILogger _logger;
        private RabbitMQEngineOptions _options;
        private IRabbitMQConnectionManager _connectionManager;

        private IKVStore _kvStore;


        public RabbitMQEventBusEngine(IOptions<RabbitMQEngineOptions> options, ILogger<RabbitMQEventBusEngine> logger, IRabbitMQConnectionManager connectionManager, IKVStore kvStore)
        {
            _logger = logger;
            _options = options.Value;
            _connectionManager = connectionManager;

            _kvStore = kvStore;

            //初始化 自拥有线程池

            //线程池开始工作

            Thread thread = new Thread(new ThreadStart(() => { }));
            thread.Start();
        }

        public async Task<bool> PublishAsync(string brokerName, EventMessage eventMessage)
        {
            //大量Request线程放入缓存池中，离开
            //缓存池内容不能丢，所以用抗击打的Redis来存储
            //注意取消息后需要从kvstore删除

            EventMessageEntity entity = new EventMessageEntity(brokerName, eventMessage);

            KVStoreResult storeResult = await _kvStore.AddAsync(entity);

            return storeResult.IsSucceeded();
        }
    }
}
