using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.Lock.Distributed;

using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace HB.FullStack.DistributedLock.Test
{
    public class DistributedLockTest : IClassFixture<ServiceFixture>
    {
        private readonly IDistributedLockManager _lockManager;
        public DistributedLockTest(ServiceFixture serviceFixture)
        {
            _lockManager = serviceFixture.ServiceProvider.GetRequiredService<IDistributedLockManager>();
        }

        [Fact]
        public async System.Threading.Tasks.Task Test1Async()
        {
            IEnumerable<string> resources = new List<string> { "aa", "bb", "cc" };

            IDistributedLock lock1 = await _lockManager.LockAsync(
                resources: resources,
                expiryTime: TimeSpan.FromSeconds(20),
                waitTime: (TimeSpan?)TimeSpan.FromSeconds(10),
                retryInterval: (TimeSpan?)TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            await Task.Delay(5 * 1000);

            lock1.Dispose();
        }
    }
}
