using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.DistributedQueue;
using Microsoft.Extensions.Logging;

namespace HB.Infrastructure.Redis.Queue
{
    public class RedisQueue : RedisEngineBase, IDistributedQueue
    {
        private ILogger _logger;

        public RedisQueue(RedisEngineOptions options, ILogger<RedisQueue> logger) : base(options, logger)
        {
            _logger = logger;
        }

        public IDistributedQueueResult InsertFront<T>(string queueName, T data)
        {
            //TODO: 要用polly来确保吗?
            throw new NotImplementedException();
        }

        public ulong Length(string queueName)
        {
            throw new NotImplementedException();
        }

        public IDistributedQueueResult PopAndPush<T>(string fromQueueName, string toQueueName)
        {
            throw new NotImplementedException();
        }

        public Task<IDistributedQueueResult> Push<T>(string queueName, T data)
        {
            throw new NotImplementedException();
        }
    }
}
