using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DistributedQueue
{
    public class IDistributedQueueResult
    {
        public int QueueLength { get; set; }
        public object Data { get; set; }
        public bool HistoryDeleted { get; set; }
        public bool HistoryReback { get; set; }
        public bool HistoryShouldWait { get; set; }

        public bool IsSucceeded()
        {
            throw new NotImplementedException();
        }
    }
}
