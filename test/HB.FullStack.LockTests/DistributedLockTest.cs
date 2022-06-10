using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.LockTests
{
    [TestClass]
    public class DistributedLockTest : BaseTestClass
    {
        [TestMethod]
        public async System.Threading.Tasks.Task Test1Async()
        {
            IEnumerable<string> resources = new List<string> { "aa", "bb", "cc" };

            IDistributedLock lock1 = await DistributedLockManager.LockAsync(
                resources: resources,
                expiryTime: TimeSpan.FromSeconds(20),
                waitTime: (TimeSpan?)TimeSpan.FromSeconds(10),
                retryInterval: (TimeSpan?)TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            await Task.Delay(5 * 1000).ConfigureAwait(false);

            lock1.Dispose();
        }
    }
}