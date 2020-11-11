using HB.Framework.Common.Entities;
using HB.Framework.KVStore;
using HB.Framework.KVStore.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    public class UserEntity : Entity
    {

        public string? UserName { get; set; }

        public DateTimeOffset CreateTime { get; set; }

        public bool Activated { get; set; }

        public UserType Type { get; set; }
    }

    class UserEntityComparer : IEqualityComparer<UserEntity>
    {
        public bool Equals([AllowNull] UserEntity x, [AllowNull] UserEntity y)
        {
            if (x == null && y == null) { return true; }
            if (x == null && y != null) { return false; }
            if (x != null && y == null) { return false; }

            return x!.Id == y!.Id
                && x.UserName == y.UserName
                && x.CreateTime == y.CreateTime
                && x.Activated == y.Activated
                && x.Type == y.Type;
        }

        public int GetHashCode([DisallowNull] UserEntity obj)
        {
            return obj.GetHashCode();
        }
    }


    public class KVStoreTest : IClassFixture<ServiceFixture>
    {
        private readonly IKVStore _kvStore;
        private readonly ITestOutputHelper _output;

        private readonly UserEntity _userEntity1 = new UserEntity()
        {
            Id = 300,
            UserName = "22222222222",
            CreateTime = DateTime.UtcNow,
            Activated = true,
            Type = UserType.Admin
        };

        private readonly UserEntity _userEntity2 = new UserEntity()
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
            _kvStore = fixture.ThrowIfNull(nameof(fixture)).KVStore;
        }

        [Fact]
        public async Task AddAndFetchAsync()
        {
            UserEntity? fetched = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity1.Id).ConfigureAwait(false);

            if (fetched != null)
            {
                await _kvStore.DeleteByKeyAsync<UserEntity>(fetched.Id, fetched.Version).ConfigureAwait(false);
            }

            await _kvStore.AddAsync(_userEntity1).ConfigureAwait(false);

            UserEntity? fetchedAgain = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity1.Id).ConfigureAwait(false);

            Assert.Equal<UserEntity>(_userEntity1, fetchedAgain!, new UserEntityComparer());
        }

        [Fact]
        public async Task AddAndUpdateAsync()
        {
            UserEntity? fetched = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id).ConfigureAwait(false);

            if (fetched == null)
            {
                await _kvStore.AddAsync(_userEntity2).ConfigureAwait(false);

                fetched = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id).ConfigureAwait(false);

                Assert.True(fetched != null);
            }

            fetched!.UserName = "Changed 1 : " + fetched.UserName;

            await _kvStore.UpdateAsync(fetched).ConfigureAwait(false);

            UserEntity? fetchedAgain = await _kvStore.GetByKeyAsync<UserEntity>(_userEntity2.Id).ConfigureAwait(false);

            Assert.True(condition: fetched.Version == fetchedAgain!.Version);

            await _kvStore.DeleteByKeyAsync<UserEntity>(_userEntity2.Id, fetchedAgain.Version).ConfigureAwait(false);

        }
    }
}
