﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using HB.FullStack.Cache;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CacheTests
{
    //TODO: Update与Update之间并行冲突
    //TODO：Update与Invalidation之间并行冲突

    [TestClass]
    public class UpdateInvalidationConcurrencyTests : BaseTestClass
    {
        /// <summary>
        /// 同时有多个线程来update
        /// </summary>
        /// <exception cref="CacheException"></exception>
        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task Timestamp_Multiple_Update_Concurrency_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            Random random = new Random(DateTime.Now.Millisecond);
            string key = "Timestamp_Multiple_Update_Concurrency_Test";

            List<Task> tasks = new List<Task> {
            UpdateTimestampCacheAsync(1, key, entryOptions, random),
            UpdateTimestampCacheAsync(2, key, entryOptions, random),
            UpdateTimestampCacheAsync(3, key, entryOptions, random),
            UpdateTimestampCacheAsync(4, key, entryOptions, random),
            UpdateTimestampCacheAsync(5, key, entryOptions, random),
            UpdateTimestampCacheAsync(6, key, entryOptions, random),
            UpdateTimestampCacheAsync(7, key, entryOptions, random),
            UpdateTimestampCacheAsync(8, key, entryOptions, random),
            UpdateTimestampCacheAsync(9, key, entryOptions, random),
            UpdateTimestampCacheAsync(10, key, entryOptions, random),
            UpdateTimestampCacheAsync(11, key, entryOptions, random),
            UpdateTimestampCacheAsync(12, key, entryOptions, random)
            };

            await Task.WhenAll(tasks);

            int newestRetrieveTaskId = 0;
            long retrieveTicks = 0;

            foreach (KeyValuePair<int, (long, string)> item in _multipleUpdates)
            {
                if (item.Value.Item1 > retrieveTicks)
                {
                    retrieveTicks = item.Value.Item1;
                    newestRetrieveTaskId = item.Key;
                }
            }

            Console.WriteLine($"任务{newestRetrieveTaskId} 最终成功");

            string predicateResult = _multipleUpdates[newestRetrieveTaskId].Item2;

            string? result = await Cache.GetStringAsync(key).ConfigureAwait(false);

            Console.WriteLine($"Result : {result}");

            Assert.IsTrue(predicateResult == result);
        }

        private readonly Dictionary<int, (long, string)> _multipleUpdates = new Dictionary<int, (long, string)>();

        private async Task UpdateTimestampCacheAsync(int taskId, string key, DistributedCacheEntryOptions entryOptions, Random random)
        {
            //模拟数据库读取
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);

            UtcNowTicks utcNowTicks = TimeUtil.UtcNowTicks;

            Console.WriteLine($"任务{taskId}, 数据库读取完成在 at {utcNowTicks.Ticks}");

            //模拟其他时间
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);

            string value = SecurityUtil.CreateUniqueToken();

            //写入Cache
            await Cache.SetStringAsync(key, value, utcNowTicks, entryOptions).ConfigureAwait(false);

            Console.WriteLine($"任务{taskId}, Try to Set Value:{value}");

            _multipleUpdates[taskId] = (utcNowTicks.Ticks, value);
        }

        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task Timestamp_Update_Invalidation_Concurrency_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            DatabaseMocker databaseMocker = new DatabaseMocker();

            //线程1从数据库获取最新数据 版本为1
            Task task1 = RetrieveDbAndUpdateCacheAsync(entryOptions, databaseMocker);

            //线程2启动更新了数据库， 数据版本变为 2. Invalidate Cache中的数据
            Task task2 = UpdateDbAndInvalidateCacheAsync(databaseMocker);

            //线程1这时才去更新Cache， 可能会将版本又变为1

            await Task.WhenAll(task1, task2);

            VersionData? cached = await Cache.GetAsync<VersionData>(databaseMocker.Guid).ConfigureAwait(false);

            Assert.IsTrue(cached == null);

            //再次updatecache

            await RetrieveDbAndUpdateCacheAsync(entryOptions, databaseMocker).ConfigureAwait(false);

            VersionData? cached2 = await Cache.GetAsync<VersionData>(databaseMocker.Guid).ConfigureAwait(false);

            Assert.IsTrue(cached2!.Version == 2);
        }

        private static async Task RetrieveDbAndUpdateCacheAsync(DistributedCacheEntryOptions entryOptions, DatabaseMocker databaseMocker)
        {
            //模拟Retrieve data
            VersionData versionData = await databaseMocker.RetrieveAsync().ConfigureAwait(false);
            UtcNowTicks timestamp = TimeUtil.UtcNowTicks;

            //模拟发生其他事情
            await Task.Delay(2000);

            //Update Cache
            await Cache.SetAsync(versionData.Guid, versionData, timestamp, entryOptions).ConfigureAwait(false);
        }

        private static async Task UpdateDbAndInvalidateCacheAsync(DatabaseMocker databaseMocker)
        {
            //模拟Update Database
            VersionData versionData = await databaseMocker.RetrieveAsync().ConfigureAwait(false);
            versionData.Version++;
            await databaseMocker.UpdateAsync(versionData).ConfigureAwait(false);
            UtcNowTicks timestamp = TimeUtil.UtcNowTicks;

            //其他事情
            await Task.Delay(100).ConfigureAwait(false);

            //Invalidate Cache
            await Cache.RemoveAsync(versionData.Guid, timestamp).ConfigureAwait(false);
        }
    }
}