using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HB.Framework.DistributedQueue
{
    public interface IDistributedQueue
    {
        Task<IDistributedQueueResult> PushAsync<T>(string queueName, T data);

        ulong Length(string queueName);


        IDistributedQueueResult PopAndPush<T>(string fromQueueName, string toQueueName);
        IDistributedQueueResult AddIntToHash(string hashName, IList<string> fields, IList<int> values);
        IDistributedQueueResult PopHistoryToQueueIfNotExistInHash<T>(string historyQueue, string queue, string hash);
        void AddGuid(string id, long expireSeconds);
        bool ExistGuid(string id);
    }
}
