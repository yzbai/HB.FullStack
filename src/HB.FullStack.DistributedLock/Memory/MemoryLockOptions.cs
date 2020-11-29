using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Lock.Memory
{
    public class MemoryLockOptions : IOptions<MemoryLockOptions>
    {
        public TimeSpan DefaultWaitTime { get; set; } = TimeSpan.FromMinutes(1);

        public int DefaultRetryIntervalMilliseconds { get; set; } = 400;

        public MemoryLockOptions Value => this;
    }
}
