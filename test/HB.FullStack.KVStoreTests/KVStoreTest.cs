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
        private readonly UserModel _userModel1 = new UserModel()
        {
            UserName = "22222222222",
            Activated = true,
            Type = UserType.Admin
        };

        private readonly UserModel _userModel2 = new UserModel()
        {
            UserName = "333333333",
            Activated = true,
            Type = UserType.Customer
        };

        [TestMethod]
        public async Task AddAndFetchAsync()
        {
            UserModel? fetched = await KVStore.GetAsync<UserModel>(_userModel1.Guid).ConfigureAwait(false);

            Assert.IsTrue(fetched == null);

            await KVStore.AddAsync(_userModel1, "xx").ConfigureAwait(false);

            UserModel? fetchedAgain = await KVStore.GetAsync<UserModel>(_userModel1.Guid).ConfigureAwait(false);

            Assert.IsTrue(Equals(_userModel1, fetchedAgain!));
        }

        [TestMethod]
        public async Task AddAndUpdateAsync()
        {
            UserModel? fetched = await KVStore.GetAsync<UserModel>(_userModel2.Guid).ConfigureAwait(false);

            if (fetched == null)
            {
                await KVStore.AddAsync(_userModel2, "xxx").ConfigureAwait(false);

                fetched = await KVStore.GetAsync<UserModel>(_userModel2.Guid).ConfigureAwait(false);

                Assert.IsTrue(fetched != null);
            }

            fetched!.UserName = "Changed 1 : " + fetched.UserName;

            await KVStore.UpdateAsync(fetched, "xxx").ConfigureAwait(false);

            UserModel? fetchedAgain = await KVStore.GetAsync<UserModel>(_userModel2.Guid).ConfigureAwait(false);

            Assert.IsTrue(condition: fetched.Version == fetchedAgain!.Version);

            await KVStore.DeleteAsync<UserModel>(_userModel2.Guid, fetchedAgain.Version).ConfigureAwait(false);
        }

        public static bool Equals([AllowNull] UserModel x, [AllowNull] UserModel y)
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