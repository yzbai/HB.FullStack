using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DistributedQueue
{
    public class IDistributedQueueResult<T>
    {
        public int QueueLength { get; set; }
        public T Data { get; set; }

        public bool IsSucceeded()
        {
            throw new NotImplementedException();
        }
    }
}
