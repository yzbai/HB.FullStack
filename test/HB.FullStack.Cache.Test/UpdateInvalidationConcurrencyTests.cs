using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

using Xunit;
using Xunit.Abstractions;

namespace HB.FullStack.Cache.Test
{
    //TODO: Update与Update之间并行冲突
    //TODO：Update与Invalidation之间并行冲突

    public class UpdateInvalidationConcurrencyTests : IClassFixture<ServiceFixture_MySql>
    {
        private readonly ICache _cache;
        private readonly ConnectionMultiplexer _redisConnection;
        private readonly int _databaseNumber;
        private readonly ITestOutputHelper _outputHelper;
        private readonly string _applicationName;

        public UpdateInvalidationConcurrencyTests(ServiceFixture_MySql serviceFixture, ITestOutputHelper outputHelper)
        {
            _cache = serviceFixture.ServiceProvider.GetRequiredService<ICache>();
            _redisConnection = ConnectionMultiplexer.Connect(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:ConnectionString"]);
            _databaseNumber = Convert.ToInt32(ConnectionMultiplexer.Connect(serviceFixture.Configuration["RedisCache:ConnectionSettings:0:DatabaseNumber"]));
            _applicationName = serviceFixture.Configuration["RedisCache:ApplicationName"];

            _outputHelper = outputHelper;
        }


        /// <summary>
        /// 同时有多个线程来update
        /// </summary>
        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task Timestamp_Multiple_Update_Concurrency_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

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

            _outputHelper.WriteLine($"任务{newestRetrieveTaskId} 最终成功");

            string predicateResult = _multipleUpdates[newestRetrieveTaskId].Item2;

            string? result = await _cache.GetStringAsync(key).ConfigureAwait(false);

            _outputHelper.WriteLine($"Result : {result}");

            Assert.True(predicateResult == result);
        }

        private Dictionary<int, (long, string)> _multipleUpdates = new Dictionary<int, (long, string)>();

        private async Task UpdateTimestampCacheAsync(int taskId, string key, DistributedCacheEntryOptions entryOptions, Random random)
        {
            //模拟数据库读取
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);

            UtcNowTicks utcNowTicks = TimeUtil.UtcNowTicks;

            _outputHelper.WriteLine($"任务{taskId}, 数据库读取完成在 at {utcNowTicks.Ticks}");


            //模拟其他时间
            await Task.Delay(random.Next(1000, 2000)).ConfigureAwait(false);

            string value = SecurityUtil.CreateUniqueToken();


            //写入Cache
            await _cache.SetStringAsync(key, value, utcNowTicks, entryOptions).ConfigureAwait(false);

            _outputHelper.WriteLine($"任务{taskId}, Try to Set Value:{value}");

            _multipleUpdates[taskId] = (utcNowTicks.Ticks, value);
        }

        [Theory]
        [InlineData(50, 40)]
        [InlineData(null, 20)]
        [InlineData(20, null)]
        [InlineData(null, null)]
        public async Task Timestamp_Update_Invalidation_Concurrency_TestAsync(int? absoluteSecondsRelativeToNow, int? slidingSeconds)
        {
            DistributedCacheEntryOptions entryOptions = new DistributedCacheEntryOptions();
            entryOptions.AbsoluteExpirationRelativeToNow = absoluteSecondsRelativeToNow == null ? null : (TimeSpan?)TimeSpan.FromSeconds(absoluteSecondsRelativeToNow.Value);
            entryOptions.SlidingExpiration = slidingSeconds == null ? null : (TimeSpan?)TimeSpan.FromSeconds(slidingSeconds.Value);

            DatabaseMocker databaseMocker = new DatabaseMocker();


            //线程1从数据库获取最新数据 版本为1
            Task task1 = RetrieveDbAndUpdateCacheAsync(entryOptions, databaseMocker);


            //线程2启动更新了数据库， 数据版本变为 2. Invalidate Cache中的数据
            Task task2 = UpdateDbAndInvalidateCacheAsync(databaseMocker);

            //线程1这时才去更新Cache， 可能会将版本又变为1


            await Task.WhenAll(task1, task2);

            VersionData? cached = await _cache.GetAsync<VersionData>(databaseMocker.Guid).ConfigureAwait(false);

            Assert.True(cached == null);

            //再次updatecache

            await RetrieveDbAndUpdateCacheAsync(entryOptions, databaseMocker).ConfigureAwait(false);

            VersionData? cached2 = await _cache.GetAsync<VersionData>(databaseMocker.Guid).ConfigureAwait(false);

            Assert.True(cached2!.Version == 2);
        }


        private async Task RetrieveDbAndUpdateCacheAsync(DistributedCacheEntryOptions entryOptions, DatabaseMocker databaseMocker)
        {
            //模拟Retrieve data
            VersionData versionData = await databaseMocker.RetrieveAsync().ConfigureAwait(false);
            UtcNowTicks timestamp = TimeUtil.UtcNowTicks;

            //模拟发生其他事情
            await Task.Delay(2000);

            //Update Cache
            await _cache.SetAsync(versionData.Guid, versionData, timestamp, entryOptions).ConfigureAwait(false);

        }

        private async Task UpdateDbAndInvalidateCacheAsync(DatabaseMocker databaseMocker)
        {
            //模拟Update Database
            VersionData versionData = await databaseMocker.RetrieveAsync().ConfigureAwait(false);
            versionData.Version++;
            await databaseMocker.UpdateAsync(versionData).ConfigureAwait(false);
            UtcNowTicks timestamp = TimeUtil.UtcNowTicks;

            //其他事情
            await Task.Delay(100).ConfigureAwait(false);

            //Invalidate Cache
            await _cache.RemoveAsync(versionData.Guid, timestamp).ConfigureAwait(false);
        }
    }

    public class DatabaseMocker
    {
        public int CurrentVerson = 1;
        public string Guid = SecurityUtil.CreateUniqueToken();

        public async Task<VersionData> RetrieveAsync()
        {
            await Task.Delay(10);
            return new VersionData { Guid = Guid, Version = CurrentVerson };
        }

        public async Task UpdateAsync(VersionData versionData)
        {
            await Task.Delay(20);
            CurrentVerson = versionData.Version;
        }
    }

    public class VersionData
    {
        public string Guid { get; set; }
        public int Version { get; set; }
    }
}
