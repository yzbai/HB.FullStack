using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.DistributedQueue
{
    public interface IDistributedQueue
    {
        Task<IDistributedQueueResult> Push<T>(string queueName, T data);

        ulong Length(string queueName);


        /// <summary>
        /// 不能保证一定是在最前面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        IDistributedQueueResult InsertFront<T>(string queueName, T data);

        IDistributedQueueResult PopAndPush<T>(string fromQueueName, string toQueueName);
        IDistributedQueueResult AddIntToHash(string hashName, IList<string> fields, IList<int> values);
        IDistributedQueueResult PopHistoryToQueueIfNotExistInHash<T>(string historyQueue, string queue, string hash);
    }
}
