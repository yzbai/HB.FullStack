using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using HB.FullStack.BaseTest;
using HB.FullStack.Lock.Memory;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.LockTests
{
    [TestClass]
    public class MemoryLockTests : BaseTestClass
    {
        [TestMethod]
        public void TestSingleLock()
        {
            var resources = Mocker.MockResourcesWithThree();

            using var @lock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(30));
            Assert.IsTrue(@lock.IsAcquired);
        }

        [TestMethod]
        public void TestOverlappingLocks()
        {
            var resources = Mocker.MockResourcesWithThree();

            using var firstLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(30));
            Assert.IsTrue(firstLock.IsAcquired);

            using var secondLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(30));
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

        private static async Task LockWorkAsync(IEnumerable<string> resources, ConcurrentBag<int> locksAcquired)
        {
            using var @lock = MemoryLockManager.Lock(
                "Test",
                    resources,
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(60));
            Globals.Logger?.LogInformation("Entering lock");
            if (@lock.IsAcquired)
            {
                locksAcquired.Add(1);
            }
            await Task.Delay(4000).ConfigureAwait(false);

            Globals.Logger?.LogInformation("Leaving lock");
        }

        [TestMethod]
        public void TestSequentialLocks()
        {
            var resources = Mocker.MockResourcesWithThree();

            using (var firstLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(30)))
            {
                Globals.Logger?.LogInformation("TestSequentialLocks  :  First Enter");

                Assert.IsTrue(firstLock.IsAcquired);
            }

            using var secondLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(30));
            Globals.Logger?.LogInformation("TestSequentialLocks  :  Second Enter");

            Assert.IsTrue(secondLock.IsAcquired);
        }

        [TestMethod]
        public void TestRenewing()
        {
            var resources = Mocker.MockResourcesWithThree();

            int extendCount;

            using (IMemoryLock? @lock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromMilliseconds(100)))
            {
                Assert.IsTrue(@lock.IsAcquired);

                Thread.Sleep(4000);

                extendCount = @lock.ExtendCount;
            }

            Assert.IsTrue(extendCount > 2);
        }

        [TestMethod]
        public void TestLockReleasedAfterTimeout()
        {
            var resources = Mocker.MockResourcesWithThree();

            using var firstLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(1));
            Assert.IsTrue(firstLock.IsAcquired);

            Thread.Sleep(550); // should cause keep alive timer to fire once
            ((MemoryLock)firstLock).StopKeepAliveTimer(); // stop the keep alive timer to simulate process crash
            Thread.Sleep(1200); // wait until the key expires from redis

            using var secondLock = MemoryLockManager.Lock("Test", resources, TimeSpan.FromSeconds(1));
            Assert.IsTrue(secondLock.IsAcquired); // Eventually the outer lock should timeout
        }
    }
}