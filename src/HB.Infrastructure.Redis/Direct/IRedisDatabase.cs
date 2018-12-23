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
        bool KeySetIfNotExist(string redisInstanceName, string key, long expireSeconds);

        void HashSetInt(string redisInstanceName, string hashName, IEnumerable<string> fields, IEnumerable<int> values);

        IEnumerable<int> HashGetInt(string redisInstanceName, string hashName, IEnumerable<string> fields);

        Task<long> PushAsync<T>(string redisInstanceName, string queueName, T data);

        T PopAndPush<T>(string redisInstanceName, string fromQueueName, string toQueueName);

        ulong QueueLength(string redisInstanceName, string queueName);

        int ScriptEvaluate(string redisInstanceName, string script, string[] keys, string[] argvs);
    }
}