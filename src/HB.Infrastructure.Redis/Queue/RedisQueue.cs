using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Common;
using HB.Framework.DistributedQueue;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace HB.Infrastructure.Redis.Queue
{
    public class RedisQueue : RedisEngineBase, IDistributedQueue
    {
        private readonly ILogger _logger;

        public RedisQueue(RedisEngineOptions options, ILogger<RedisQueue> logger) : base(options, logger)
        {
            _logger = logger;
        }

        public IDistributedQueueResult AddIntToHash(string hashName, IList<string> fields, IList<int> values)
        {
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

        public IDistributedQueueResult PopHistoryToQueueIfNotExistInHash<T>(string historyQueue, string queue, string hash)
        {
            throw new NotImplementedException();
        }

        public async Task<IDistributedQueueResult> PushAsync<T>(string queueName, T data)
        {
            RedisValue redisValue = DataConverter.Serialize<T>(data);
            IDatabase database = GetQueueDatabase();

            long length = await database.ListLeftPushAsync(queueName, DataConverter.Serialize(data), When.Always).ConfigureAwait(false);

            IDistributedQueueResult result = IDistributedQueueResult.Succeed;
            result.QueueLength = length;

            return result;
        }
    }
}
