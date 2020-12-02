using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DistributedQueue
{
    public class IDistributedQueueResult
    {
        private static IDistributedQueueResult _succeed = new IDistributedQueueResult() { IsSucceeded = true };
        private static IDistributedQueueResult _failed = new IDistributedQueueResult() { IsSucceeded = false };

        public object ResultData { get; set; }

        public bool HistoryDeleted { get; set; }
        public bool HistoryReback { get; set; }
        public bool HistoryShouldWait { get; set; }

        public bool IsSucceeded { get; set; }

        public static IDistributedQueueResult Succeed { get { return _succeed; } }

        public static IDistributedQueueResult Failed { get { return _failed; } }

        public long QueueLength { get; set; }
    }
}
