using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using HB.Framework.Common;
using HB.Infrastructure.Redis.Direct;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.Redis.Test
{
    public class DirectRedisTest : IClassFixture<ServiceFixture>
    {
        private Fixture _fixture = new Fixture();

        private ITestOutputHelper _output;

        private IRedisDatabase _redis;

        public DirectRedisTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            _output = testOutputHelper;
            _redis = serviceFixture.Redis;
        }

        [Theory]
        [InlineData("Direct_Redis")]
        public void KeySetIfNotExist_SetKeyTwice_FirstReturnTrueAndSecondReturnFalse(string instanceName)
        {
            string key = SecurityUtil.CreateUniqueToken();

            bool first = _redis.KeySetIfNotExist(instanceName, key, 100);

            bool second = _redis.KeySetIfNotExist(instanceName, key, 100);

            Assert.True(first && !second);
        }

        [Theory]
        [InlineData("Direct_Redis")]
        public void HashSetInt_SetAndGet_ReturnSameTrue(string instanceName)
        {
            string hashName = _fixture.Create<string>();
            IEnumerable<string> fields = _fixture.CreateMany<string>();
            IEnumerable<int> values = _fixture.CreateMany<int>(fields.Count());

            _redis.HashSetInt(instanceName, hashName, fields, values);

            IEnumerable<int> rtValue = _redis.HashGetInt(instanceName, hashName, fields);

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
