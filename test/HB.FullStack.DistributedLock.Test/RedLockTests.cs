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

using StackExchange.Redis;

using Xunit;

namespace HB.FullStack.DistributedLock.Test
{
    public class RedLockTests : IClassFixture<ServiceFixture>
    {

        private readonly IDistributedLockManager _lockManager;

        private readonly ILogger _logger;
        public RedLockTests(ServiceFixture serviceFixture)
        {
            _lockManager = serviceFixture.ServiceProvider.GetRequiredService<IDistributedLockManager>();
            _logger = GlobalSettings.Logger;
        }


        [Fact]
        public async Task TestSingleLockAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (IDistributedLock redisLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(30), null, null).ConfigureAwait(false))
            {
                Assert.True(redisLock.IsAcquired);
            }
        }

        [Fact]
        public async Task TestOverlappingLocksAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                Assert.True(firstLock.IsAcquired);

                using (var secondLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false))
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

            await Task.WhenAll(tasks);

            Assert.True(locksAcquired.Count == 6);
        }

        async Task LockWorkAsync(IEnumerable<string> resources, ConcurrentBag<int> locksAcquired)
        {
            using (IDistributedLock redisLock = await _lockManager.LockAsync(
                    resources,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(60),
                    TimeSpan.FromSeconds(0.5)).ConfigureAwait(false))
            {
                _logger.LogInformation("Entering lock");
                if (redisLock.IsAcquired)
                {
                    locksAcquired.Add(1);
                }
                await Task.Delay(4000);

                _logger.LogInformation("Leaving lock");
            }
        }

        [Fact]
        public async Task TestSequentialLocksAsync()
        {

            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                _logger.LogInformation("TestSequentialLocks  :  First Enter");

                Assert.True(firstLock.IsAcquired);
            }

            using (var secondLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(30)).ConfigureAwait(false))
            {
                _logger.LogInformation("TestSequentialLocks  :  Second Enter");

                Assert.True(secondLock.IsAcquired);
            }
        }

        [Fact]
        public async Task TestRenewingAsync()
        {

            var resources = Mocker.MockResourcesWithThree();

            int extendCount;

            using (IDistributedLock redisLock = await _lockManager.LockAsync(resources, TimeSpan.FromMilliseconds(100)))
            {
                Assert.True(redisLock.IsAcquired);

                Thread.Sleep(4000);

                extendCount = redisLock.ExtendCount;
            }

            Assert.True(extendCount > 2);

        }

        [Fact]
        public async Task TestLockReleasedAfterTimeoutAsync()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(1)))
            {
                Assert.True(firstLock.IsAcquired);

                Thread.Sleep(550); // should cause keep alive timer to fire once
                //((RedisLock)firstLock).StopKeepAliveTimer(); // stop the keep alive timer to simulate process crash

                firstLock.Dispose();

                Thread.Sleep(1200); // wait until the key expires from redis

                using (var secondLock = await _lockManager.LockAsync(resources, TimeSpan.FromSeconds(1)))
                {
                    Assert.True(secondLock.IsAcquired); // Eventually the outer lock should timeout
                }
            }
        }


        [Fact]
        public async Task TestCancelBlockingLockAsync()
        {
            var cts = new CancellationTokenSource();

            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = await _lockManager.LockAsync(
                resources,
                TimeSpan.FromSeconds(300),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(1)))
            {
                Assert.True(firstLock.IsAcquired);

                cts.CancelAfter(TimeSpan.FromSeconds(5));



                await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                {
                    using var secondLock = await _lockManager.LockAsync(
                        resources,
                        TimeSpan.FromSeconds(30),
                        TimeSpan.FromSeconds(100),
                        TimeSpan.FromSeconds(1),
                        cts.Token);
                    // should never get here
                    Assert.True(false);
                }).ConfigureAwait(false);
            }

        }
    }
}
