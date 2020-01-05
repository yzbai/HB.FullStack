using System.Collections.Generic;
using System.Threading.Tasks;

namespace HB.Infrastructure.Redis.Direct
{
    public interface IRedisDatabase
    {
        /// <summary>
        /// 可以用来防重,一定时间内
        /// </summary>
        /// <param name="redisInstanceName"></param>
        /// <param name="key"></param>
        /// <param name="expireSeconds"></param>
        /// <returns></returns>
        Task<bool> KeySetIfNotExistAsync(string redisInstanceName, string key, long expireSeconds);

        Task HashSetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values);

        Task<IEnumerable<int>> HashGetIntAsync(string redisInstanceName, string hashName, IEnumerable<string> fields);

        Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data) where T : class;

        Task<T> PopAndPushAsync<T>(string redisInstanceName, string fromQueueName, string toQueueName) where T : class;

        Task<ulong> QueueLengthAsync(string redisInstanceName, string queueName);

        Task<int> ScriptEvaluateAsync(string redisInstanceName, string script, string[] keys, string[] argvs);
    }
}