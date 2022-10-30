using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.Lock.Distributed;
using HB.Infrastructure.Redis.DistributedLock;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using StackExchange.Redis;

namespace HB.FullStack.LockTests
{
    [TestClass]
    public class RedLockTests : BaseTestClass
    {
        [TestMethod]
        public async Task TestSingleLockAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using IDistributedLock redisLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(30), null, null).ConfigureAwait(false);
            Assert.IsTrue(redisLock.IsAcquired);
        }

        [TestMethod]
        public async Task TestOverlappingLocksAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using var firstLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            Assert.IsTrue(firstLock.IsAcquired);

            using var secondLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            Assert.IsFalse(secondLock.IsAcquired);
        }

        [TestMethod]
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

            Assert.IsTrue(locksAcquired.Count == 6);
        }

        private async Task LockWorkAsync(IEnumerable<string> resources, ConcurrentBag<int> locksAcquired)
        {
            using IDistributedLock redisLock = await DistributedLockManager.LockAsync(
                    resources,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(60),
                    TimeSpan.FromSeconds(0.5)).ConfigureAwait(false);
            Globals.Logger?.LogInformation("Entering lock");
            if (redisLock.IsAcquired)
            {
                locksAcquired.Add(1);
            }
            await Task.Delay(4000).ConfigureAwait(false);

            Globals.Logger?.LogInformation("Leaving lock");
        }

        [TestMethod]
        public async Task TestSequentialLocksAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Globals.Logger?.LogInformation("TestSequentialLocks  :  First Enter");

                Assert.IsTrue(firstLock.IsAcquired);
            }

            using var secondLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false);
            Globals.Logger?.LogInformation("TestSequentialLocks  :  Second Enter");

            Assert.IsTrue(secondLock.IsAcquired);
        }

        [TestMethod]
        public async Task TestRenewingAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            int extendCount;

            using (IDistributedLock redisLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromMilliseconds(100)).ConfigureAwait(false))
            {
                Assert.IsTrue(redisLock.IsAcquired);

                Thread.Sleep(4000);

                extendCount = redisLock.ExtendCount;
            }

            Assert.IsTrue(extendCount > 2);
        }

        [TestMethod]
        public async Task TestLockReleasedAfterTimeoutAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using var firstLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Assert.IsTrue(firstLock.IsAcquired);

            Thread.Sleep(550); // should cause keep alive timer to fire once
                               //((RedisLock)firstLock).StopKeepAliveTimer(); // stop the keep alive timer to simulate process crash

            firstLock.Dispose();

            Thread.Sleep(1200); // wait until the key expires from redis

            using var secondLock = await DistributedLockManager.LockAsync(resources, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Assert.IsTrue(secondLock.IsAcquired); // Eventually the outer lock should timeout
        }

        [TestMethod]
        public async Task TestCancelBlockingLockAsync()
        {
            using var cts = new CancellationTokenSource();
            var resources = Mocker.MockResourcesWithThree();

            using var firstLock = await DistributedLockManager.LockAsync(
                resources,
                TimeSpan.FromSeconds(300),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            Assert.IsTrue(firstLock.IsAcquired);

            cts.CancelAfter(TimeSpan.FromSeconds(5));

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
            {
                using var secondLock = await DistributedLockManager.LockAsync(
                    resources,
                    TimeSpan.FromSeconds(30),
                    TimeSpan.FromSeconds(100),
                    TimeSpan.FromSeconds(1),
                    false,
                    cts.Token).ConfigureAwait(false);
                // should never get here
                Assert.IsTrue(false);
            }).ConfigureAwait(false);
        }
    }
}