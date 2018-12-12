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

        IDistributedQueueResult PopAndHistory<T>(string fromQueueName, string historyName);
        IDistributedQueueResult DeleteHistory<T>(string historyName, IList<string> ids);
    }
}
