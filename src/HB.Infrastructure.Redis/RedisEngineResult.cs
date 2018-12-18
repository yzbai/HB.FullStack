using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Infrastructure.Redis
{
    public class RedisEngineResult
    {
        public bool HistoryDeleted { get; set; }
        public bool HistoryReback { get; set; }
        public bool HistoryShouldWait { get; set; }
        public object ResultData { get; set; }

        public bool IsSucceeded()
        {
            throw new NotImplementedException();
        }
    }
}
