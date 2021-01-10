using HB.FullStack;
using HB.FullStack.KVStore;
using HB.FullStack.KVStore.Entities;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

    [KVStore]
    public class UserEntity : KVStoreEntity
    {

        public string? UserName { get; set; }

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

            return x!.Guid == y!.Guid
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


    public class KVStoreTest : IClassFixture<ServiceFixture_MySql>
    {
        private readonly IKVStore _kvStore;
        private readonly ITestOutputHelper _output;

        private readonly UserEntity _userEntity1 = new UserEntity()
        {
            UserName = "22222222222",
            Activated = true,
            Type = UserType.Admin
        };

        private readonly UserEntity _userEntity2 = new UserEntity()
        {
            UserName = "333333333",
            Activated = true,
            Type = UserType.Customer
        };

        public KVStoreTest(ITestOutputHelper output, ServiceFixture_MySql fixture)
        {
            _output = output;
            _kvStore = fixture.ServiceProvider.GetRequiredService<IKVStore>();
        }

        /// <summary>
        /// AddAndFetchAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        [Fact]
        public async Task AddAndFetchAsync()
        {
            UserEntity? fetched = await _kvStore.GetAsync<UserEntity>(_userEntity1.Guid).ConfigureAwait(false);

            Assert.True(fetched == null);

            await _kvStore.AddAsync(_userEntity1, "xx").ConfigureAwait(false);

            UserEntity? fetchedAgain = await _kvStore.GetAsync<UserEntity>(_userEntity1.Guid).ConfigureAwait(false);

            Assert.Equal<UserEntity>(_userEntity1, fetchedAgain!, new UserEntityComparer());
        }

        /// <summary>
        /// AddAndUpdateAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="KVStoreException"></exception>
        [Fact]
        public async Task AddAndUpdateAsync()
        {
            UserEntity? fetched = await _kvStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

            if (fetched == null)
            {
                await _kvStore.AddAsync(_userEntity2, "xxx").ConfigureAwait(false);

                fetched = await _kvStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

                Assert.True(fetched != null);
            }

            fetched!.UserName = "Changed 1 : " + fetched.UserName;

            await _kvStore.UpdateAsync(fetched, "xxx").ConfigureAwait(false);

            UserEntity? fetchedAgain = await _kvStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

            Assert.True(condition: fetched.Version == fetchedAgain!.Version);

            await _kvStore.DeleteAsync<UserEntity>(_userEntity2.Guid, fetchedAgain.Version).ConfigureAwait(false);

        }

        //[Fact]
        //public async Task AddOrUpdateTestAsync()
        //{
        //    IEnumerable<UserEntity?> alls = await _kvStore.GetAllAsync<UserEntity>().ConfigureAwait(false);

        //    IEnumerable<int> results = await _kvStore.AddOrUpdateAsync(new List<UserEntity> { _userEntity1, _userEntity2 }, "sfas").ConfigureAwait(false);

        //    Assert.True(results.ElementAt(0) == 0);
        //    Assert.True(results.ElementAt(1) == 0);

        //    results = await _kvStore.AddOrUpdateAsync(new List<UserEntity> { _userEntity1, _userEntity2 }, "sfas").ConfigureAwait(false);

        //    Assert.True(results.ElementAt(0) == 1);
        //    Assert.True(results.ElementAt(1) == 1);

        //    int newVersion = await _kvStore.AddOrUpdateAsync(_userEntity1, "sfas").ConfigureAwait(false);

        //    Assert.True(newVersion == 2);

        //    IEnumerable<UserEntity?> fecheds = await _kvStore.GetAsync<UserEntity>(new string[] { _userEntity1.Guid, _userEntity2.Guid }).ConfigureAwait(false);

        //    Assert.True(fecheds.ElementAt(0)!.Version == 2);
        //    Assert.True(fecheds.ElementAt(1)!.Version == 1);
        //}
    }
}
