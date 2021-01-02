using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock.Distributed;
using HB.FullStack.Lock.Memory;
using HB.Infrastructure.Redis.DistributedLock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using StackExchange.Redis;

using Xunit;

namespace HB.FullStack.DistributedLock.Test
{
    public class MemoryLockTests : IClassFixture<ServiceFixture_MySql>
    {

        private readonly IMemoryLockManager _lockManager;

        private readonly ILogger? _logger;
        public MemoryLockTests(ServiceFixture_MySql serviceFixture)
        {
            _lockManager = serviceFixture.ServiceProvider.GetRequiredService<IMemoryLockManager>();
            _logger = GlobalSettings.Logger;
        }


        [Fact]
        public void TestSingleLock()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var @lock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
            {
                Assert.True(@lock.IsAcquired);
            }
        }

        [Fact]
        public void TestOverlappingLocks()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
            {
                Assert.True(firstLock.IsAcquired);

                using (var secondLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
                {
                    Assert.False(secondLock.IsAcquired);
                }
            }
        }

        [Fact]
        public async Task TestBlockingConcurrentLocksAsync()
        {
            ConcurrentBag<int> locksAcquired = new ConcurrentBag<int>();


            var resources = Mocker.MockResourcesWithThree();

            var tasks = new List<Task>();

            for (var i = 0; i < 6; i++)
            {
                tasks.Add(LockWorkAsync(resources, locksAcquired));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            Assert.True(locksAcquired.Count == 6);
        }

        async Task LockWorkAsync(IEnumerable<string> resources, ConcurrentBag<int> locksAcquired)
        {
            using (var @lock = _lockManager.Lock(
                "Test",
                    resources,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(60)))
            {
                _logger.LogInformation("Entering lock");
                if (@lock.IsAcquired)
                {
                    locksAcquired.Add(1);
                }
                await Task.Delay(4000).ConfigureAwait(false);

                _logger.LogInformation("Leaving lock");
            }
        }

        [Fact]
        public void TestSequentialLocks()
        {

            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
            {
                _logger.LogInformation("TestSequentialLocks  :  First Enter");

                Assert.True(firstLock.IsAcquired);
            }

            using (var secondLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
            {
                _logger.LogInformation("TestSequentialLocks  :  Second Enter");

                Assert.True(secondLock.IsAcquired);
            }
        }

        [Fact]
        public void TestRenewing()
        {

            var resources = Mocker.MockResourcesWithThree();

            int extendCount;

            using (var @lock = _lockManager.Lock("Test", resources, TimeSpan.FromMilliseconds(100)))
            {
                Assert.True(@lock.IsAcquired);

                Thread.Sleep(4000);

                extendCount = @lock.ExtendCount;
            }

            Assert.True(extendCount > 2);

        }

        [Fact]
        public void TestLockReleasedAfterTimeout()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(1)))
            {
                Assert.True(firstLock.IsAcquired);

                Thread.Sleep(550); // should cause keep alive timer to fire once
                ((MemoryLock)firstLock).StopKeepAliveTimer(); // stop the keep alive timer to simulate process crash
                Thread.Sleep(1200); // wait until the key expires from redis

                using (var secondLock = _lockManager.Lock("Test", resources, TimeSpan.FromSeconds(1)))
                {
                    Assert.True(secondLock.IsAcquired); // Eventually the outer lock should timeout
                }
            }
        }
    }
}
