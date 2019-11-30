using HB.Framework.KVStore;
using HB.Framework.KVStore.Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.Redis.Test
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


    public class KVStoreTest : IClassFixture<ServiceFixture>
    {
        private IKVStore _kvStore;
        private readonly ITestOutputHelper _output;

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

        public KVStoreTest(ITestOutputHelper output, ServiceFixture fixture)
        {
            _output = output;
            _kvStore = fixture.KVStore;
        }

        [Fact]
        public async Task AddAndFetchAsync()
        {
            UserEntity fetched = _kvStore.GetByKey<UserEntity>(_userEntity1.Id);

            if (fetched != null)
            {
                _kvStore.DeleteByKey<UserEntity>(fetched.Id, fetched.Version);
            }

            await _kvStore.AddAsync<UserEntity>(_userEntity1);

            UserEntity fetchedAgain = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity1.Id);

            Assert.Equal<UserEntity>(_userEntity1, fetchedAgain, new UserEntityComparer());
        }

        [Fact]
        public async Task AddAndUpdateAsync()
        {
            UserEntity fetched = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id);

            if (fetched == null)
            {
                await _kvStore.AddAsync<UserEntity>(_userEntity2);

                fetched = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id);

                Assert.True(fetched != null);
            }

            fetched.UserName = "Changed 1 : " + fetched.UserName;

            await _kvStore.UpdateAsync(fetched);

            UserEntity fetchedAgain = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id);

            Assert.True(fetched.Version == fetchedAgain.Version);

            await _kvStore.DeleteByKeyAsync<UserEntity>(_userEntity2.Id, fetchedAgain.Version);

        }
    }
}
