using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using HB.FullStack.Common;
using HB.Infrastructure.Redis.Direct;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.Redis.Test
{
    public class DirectRedisTest : IClassFixture<ServiceFixture>
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly ITestOutputHelper _output;

        private readonly IRedisDatabase _redis;

        public DirectRedisTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;
            _redis = serviceFixture.ThrowIfNull(nameof(serviceFixture)).RedisDatabase;
        }

        [Theory]
        [InlineData("AHabit_Direct_Redis")]
        public async Task KeySetIfNotExist_SetKeyTwice_FirstReturnTrueAndSecondReturnFalseAsync(string instanceName)
        {
            string key = SecurityUtil.CreateUniqueToken();

            bool first = await _redis.KeySetIfNotExistAsync(instanceName, key, 100).ConfigureAwait(false);

            bool second = await _redis.KeySetIfNotExistAsync(instanceName, key, 100).ConfigureAwait(false);

            Assert.True(first && !second);
        }

        [Theory]
        [InlineData("AHabit_Direct_Redis")]
        public async Task HashSetInt_SetAndGet_ReturnSameTrueAsync(string instanceName)
        {
            string hashName = _fixture.Create<string>();
            IEnumerable<string> fields = _fixture.CreateMany<string>();
            IEnumerable<int> values = _fixture.CreateMany<int>(fields.Count());

            await _redis.HashSetIntAsync(instanceName, hashName, fields, values).ConfigureAwait(false);

            IEnumerable<int> rtValue = await _redis.HashGetIntAsync(instanceName, hashName, fields).ConfigureAwait(false);

            Assert.True(values.Count() == rtValue.Count());

            bool haveNotSame = false;

            for (int i = 0; i < values.Count(); ++i)
            {
                if (values.ElementAt(i) != rtValue.ElementAt(i))
                {
                    haveNotSame = true;
                    break;
                }
            }

            Assert.False(haveNotSame);
        }
    }
}
