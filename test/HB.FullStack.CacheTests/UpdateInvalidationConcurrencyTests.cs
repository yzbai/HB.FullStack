using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using HB.FullStack.BaseTest;
using HB.FullStack.Cache;
using HB.FullStack.Cache.CacheItems;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CacheTests
{
    //TODO: Update与Update之间并行冲突
    //TODO：Update与Invalidation之间并行冲突

    public class VersionData
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public long Timestamp { get; set; } = TimeUtil.Timestamp;
    }

    [TestClass]
    public class UpdateInvalidationConcurrencyTests : BaseTestClass
    {
        public UpdateInvalidationConcurrencyTests() : base(DbEngineType.SQLite)
        {
        }

        /// <summary>
        /// 同时有多个线程同时来update，最后最大Timestamp获胜
        /// </summary>
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

            //得到Timestamp最大的那个
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

            long utcNowTicks = TimeUtil.Timestamp;

            Console.WriteLine($"任务{taskId}, 数据库读取完成在 at {utcNowTicks}");

            //模拟其他时间
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);

            string value = SecurityUtil.CreateUniqueToken();

            //写入Cache
            await Cache.SetStringAsync(key, value, utcNowTicks, entryOptions).ConfigureAwait(false);

            Console.WriteLine($"任务{taskId}, Try to Set Value:{value}");

            _multipleUpdates[taskId] = (utcNowTicks, value);
        }

        [TestMethod]
        [DataRow(50, 40)]
        [DataRow(null, 20)]
        [DataRow(20, null)]
        [DataRow(null, null)]
        public async Task Timestamp_Update_Invalidation_Concurrency_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            //Prepare Data
            DatabaseMocker dbMocker = new DatabaseMocker();
            VersionData versionData = new VersionData();
            dbMocker.Add(versionData);

            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value),
                SlidingExpiration = slidingSeconds == null ? null : TimeSpan.FromSeconds(slidingSeconds.Value)
            };

            //DatabaseMocker databaseMocker = new DatabaseMocker(SecurityUtil.CreateUniqueToken());

            //线程1从数据库获取最新数据 版本为1
            Task task1 = RetrieveDbAndUpdateCacheAsync(entryOptions, dbMocker);

            //线程2启动更新了数据库， 数据版本变为 2. Invalidate Cache中的数据
            Task task2 = UpdateDbAndInvalidateCacheAsync(dbMocker, entryOptions);

            //线程1这时才去更新Cache， 可能会将版本又变为1

            await Task.WhenAll(task1, task2);
        }

        private async Task<VersionData> RetrieveDbAndUpdateCacheAsync(DistributedCacheEntryOptions entryOptions, DatabaseMocker dbMocker)
        {
            //模拟Retrieve data
            VersionData versionData = await dbMocker.RetrieveAsync();

            bool rt = await Cache.SetAsync(versionData.Id.ToString(), versionData, versionData.Timestamp, entryOptions).ConfigureAwait(false);
            Assert.IsTrue(rt);

            //先update
            await dbMocker.UpdateAsync(versionData).ConfigureAwait(false);

            //模拟发生其他事情,这个时候Task去修改了数据库，并且更新了Cache
            await Task.Delay(7000);

            //Update Cache
            //Cache.SetAsync<VersionData>()
            bool rt2 = await Cache.SetAsync(versionData.Id.ToString(), versionData, versionData.Timestamp, entryOptions).ConfigureAwait(false);

            Assert.IsFalse(rt2);

            return versionData;
        }

        private async Task UpdateDbAndInvalidateCacheAsync(DatabaseMocker databaseMocker, DistributedCacheEntryOptions entryOptions)
        {
            //模拟Update Database
            VersionData versionData = await databaseMocker.RetrieveAsync().ConfigureAwait(false);
            await Task.Delay(500);
            bool rt = await Cache.SetAsync(versionData.Id.ToString(), versionData, versionData.Timestamp, entryOptions).ConfigureAwait(false);
            Assert.IsFalse(rt);//因为RetrieveDbAndUpdateCacheAsync中，已经添加了，重复的timestamp导致添加不成功

            //模拟发生其他事情
            await Task.Delay(1000);

            //后update. Task1 这时还在Delay中
            await databaseMocker.UpdateAsync(versionData).ConfigureAwait(false);
            bool rt2 = await Cache.SetAsync(versionData.Id.ToString(), versionData, versionData.Timestamp, entryOptions).ConfigureAwait(false);
            Assert.IsTrue(rt2);
        }
    }

    public class DatabaseMocker
    {
        private Guid? _id;
        private long? _timestamp;

        public void Add(VersionData versionData)
        {
            _id = versionData.Id;
            _timestamp = versionData.Timestamp;
        }

        public async Task<VersionData> RetrieveAsync()
        {
            await Task.Delay(10);
            return new VersionData { Id = _id!.Value, Timestamp = _timestamp!.Value };
        }

        public async Task UpdateAsync(VersionData versionData)
        {
            await Task.Delay(20);
            versionData.Timestamp = TimeUtil.Timestamp;
        }
    }
}