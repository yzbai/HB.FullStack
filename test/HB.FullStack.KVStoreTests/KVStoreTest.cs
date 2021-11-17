using HB.FullStack;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.KVStoreTests
{
    [TestClass]
    public class KVStoreTest : BaseTestClass
    {
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

        [TestMethod]
        public async Task AddAndFetchAsync()
        {
            UserEntity? fetched = await KVStore.GetAsync<UserEntity>(_userEntity1.Guid).ConfigureAwait(false);

            Assert.IsTrue(fetched == null);

            await KVStore.AddAsync(_userEntity1, "xx").ConfigureAwait(false);

            UserEntity? fetchedAgain = await KVStore.GetAsync<UserEntity>(_userEntity1.Guid).ConfigureAwait(false);

            Assert.IsTrue(Equals(_userEntity1, fetchedAgain!));
        }

        [TestMethod]
        public async Task AddAndUpdateAsync()
        {
            UserEntity? fetched = await KVStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

            if (fetched == null)
            {
                await KVStore.AddAsync(_userEntity2, "xxx").ConfigureAwait(false);

                fetched = await KVStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

                Assert.IsTrue(fetched != null);
            }

            fetched!.UserName = "Changed 1 : " + fetched.UserName;

            await KVStore.UpdateAsync(fetched, "xxx").ConfigureAwait(false);

            UserEntity? fetchedAgain = await KVStore.GetAsync<UserEntity>(_userEntity2.Guid).ConfigureAwait(false);

            Assert.IsTrue(condition: fetched.Version == fetchedAgain!.Version);

            await KVStore.DeleteAsync<UserEntity>(_userEntity2.Guid, fetchedAgain.Version).ConfigureAwait(false);
        }

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
    }
}