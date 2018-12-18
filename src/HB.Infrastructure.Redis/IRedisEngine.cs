using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis
{
    public interface IRedisEngine
    {
        bool KeySetIfNotExist(string redisInstanceName, string id, long expireSeconds);

        void HashSetInt(string redisInstanceName, string hashName, IList<string> fields, IList<int> values);

        Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data);

        T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName);

        ulong QueueLength(string redisInstanceName, string queueName);



        //TODO: 提到RabbitMQ去，太业务化了
        RedisEngineResult PopAndPushIfNotExist<T>(string redisInstanceName, string historyQueue, string queue, string hashName);
    }
}