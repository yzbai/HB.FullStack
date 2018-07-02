using HB.Framework.KVStore;
using HB.Framework.KVStore.Entity;
using HB.Infrastructure.Redis.Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace HB.PresentFish.Tools
{
    public enum UserType
    {
        Customer,
        Admin
    }

    public class UserEntity : KVStoreEntity
    {
        [KVStoreKey]
        public long Id { get; set; }

        public string UserName { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public bool Activated { get; set; }

        public UserType Type { get; set; }
    }

    public class UserEntityComparer : IEqualityComparer<UserEntity>
    {
        public bool Equals(UserEntity x, UserEntity y)
        {
            if (x == null && y == null) { return true; }
            if (x == null && y != null) { return false; }
            if (x != null && y == null) { return false; }

            return x.Id == y.Id 
                && x.UserName == y.UserName 
                && x.CreateTime == y.CreateTime 
                && x.Activated == y.Activated 
                && x.Type == y.Type;
        }

        public int GetHashCode(UserEntity obj)
        {
            return obj.GetHashCode();
        }
    }


    public class KVStoreTest : IClassFixture<TestFixture>
    {
        private IKVStore _kvStore;
        private ITestOutputHelper _output;
        private TestFixture _fixture;

        private UserEntity _userEntity1 = new UserEntity()
        {
            Id = 300,
            UserName = "22222222222",
            CreateTime = DateTime.UtcNow,
            Activated = true,
            Type = UserType.Admin
        };

        private UserEntity _userEntity2 = new UserEntity()
        {
            Id = 400,
            UserName = "333333333",
            CreateTime = DateTime.UtcNow,
            Activated = true,
            Type = UserType.Customer
        };

        public KVStoreTest(ITestOutputHelper output, TestFixture fixture)
        {
            _output = output;
            _fixture = fixture;
            _kvStore = _fixture.GetKVStore();
        }

        [Fact]
        public void addAndFetch()
        {
            UserEntity fetched = _kvStore.GetById<UserEntity>(_userEntity1.Id);

            if (fetched != null)
            {
                KVStoreResult r = _kvStore.DeleteById<UserEntity>(fetched.Id, fetched.Version);
                Assert.True(r.IsSucceeded());
            }

            KVStoreResult result = _kvStore.AddAsync<UserEntity>(_userEntity1).Result;

            Assert.True(result.IsSucceeded());

            UserEntity fetchedAgain = _kvStore.GetByIdAsync<UserEntity>(_userEntity1.Id).Result;

            Assert.Equal<UserEntity>(_userEntity1, fetchedAgain, new UserEntityComparer());
        }

        [Fact]
        public void addAndUpdate()
        {
            KVStoreResult result = KVStoreResult.Succeeded();

            UserEntity fetched = _kvStore.GetByIdAsync<UserEntity>(_userEntity2.Id).Result;

            if (fetched == null)
            {
                result = _kvStore.AddAsync<UserEntity>(_userEntity2).Result;

                Assert.True(result.IsSucceeded());

                fetched = _kvStore.GetByIdAsync<UserEntity>(_userEntity2.Id).Result;

                Assert.True(fetched != null);
            }

            fetched.UserName = "Changed 1 : " + fetched.UserName;

            result = _kvStore.UpdateAsync(fetched).Result;

            Assert.True(result.IsSucceeded());

            UserEntity fetchedAgain = _kvStore.GetByIdAsync<UserEntity>(_userEntity2.Id).Result;

            Assert.True(fetched.Version == fetchedAgain.Version);

            result = _kvStore.DeleteByIdAsync<UserEntity>(_userEntity2.Id, fetchedAgain.Version).Result;

            Assert.True(result.IsSucceeded());
        }
    }
}
