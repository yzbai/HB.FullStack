using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
