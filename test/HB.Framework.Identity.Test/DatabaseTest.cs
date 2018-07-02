using HB.Component.Identity.Entity;
using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace HB.Component.Identity.Test
{

    public class DatabaseTest : BizWithDbTransaction, IClassFixture<DatabaseTestFixture>
    {
        private ITestOutputHelper _output;
        private DatabaseTestFixture _fixture;
        private IDatabase _db;
        private IList<User> _userList;
        private IList<Role> _roleList;

        public DatabaseTest(ITestOutputHelper output, DatabaseTestFixture databaseTestFixture) : base(databaseTestFixture.GetDatabase())
        {
            _output = output;
            _fixture = databaseTestFixture;
            _db = _fixture.GetDatabase();

            _userList = DataMocker.MockUsers();
            _roleList = DataMocker.MockRoles();
        }

        [Fact]
        public void AddUsers()
        {
            DbTransactionContext transContext = BeginTransaction<User>();
            DatabaseResult result = DatabaseResult.Failed();

            foreach (User item in _userList)
            {
                result = _db.AddAsync(item, transContext).Result;

                if (!result.IsSucceeded())
                {
                    Rollback(transContext);
                    break;
                }
            }

            Commit(transContext);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void AddRole()
        {
            DbTransactionContext transContext = BeginTransaction<Role>();
            DatabaseResult result = DatabaseResult.Succeeded();

            foreach (Role item in _roleList)
            {
                long count = _db.Count<Role>(r => r.Name == item.Name, transContext);

                if (count != 0)
                {
                    continue;
                }

                result = _db.AddAsync(item, transContext).Result;

                if (!result.IsSucceeded())
                {
                    Rollback(transContext);
                    break;
                }
            }

            Commit(transContext);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void AddSomeUserClaims()
        {
            int userCount = _userList.Count;
            DatabaseResult result = DatabaseResult.Failed();

            for (int i = 1; i < userCount; i += 39)
            {
                UserClaim uc = new UserClaim() { UserId = i, ClaimValue = "Nothing", ClaimType = "HB.Nothing" };
                result = _db.AddAsync(uc).Result;
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void AddSomeUserRole()
        {
            int userCount = _userList.Count;
            DatabaseResult result = DatabaseResult.Failed();

            int roleId = 1;

            for (int i = 1; i < userCount; i += 7)
            {
                UserRole ur = new UserRole() { RoleId = roleId++, UserId = i };

                if (roleId == 5) { roleId = 1; }

                result = _db.AddAsync(ur).Result;
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void GetAdminUsers()
        {
            Select<User> select = _db.Select<User>().select(u => u.UserName).select(u => u.Mobile);

            From<UserRole> from = _db.From<UserRole>()
                                          .LeftJoin<Role>((ur, r) => ur.RoleId == r.Id)
                                          .LeftJoin<User>((ur, u) => ur.UserId == u.Id);

            Where<UserRole> where = _db.Where<UserRole>().And<Role>(r => r.Name == "Admin");

            IList<User> resultList = _db.RetrieveAsync(select, from, where).Result;

            if (resultList.Count > 0)
            {
                var roleList = _db.Retrieve<Role>(r => r.Name == "Admin");

                Assert.True(roleList.Count == 1);

                Role adminRole = roleList[0];

                foreach (User item in resultList)
                {
                    _output.WriteLine(DataConverter.ToJson(item));

                    var lst = _db.Retrieve<UserRole>(ur => ur.UserId == item.Id && ur.RoleId == adminRole.Id);

                    Assert.True(lst.Count >= 1);
                }
            }
        }

        [Fact]
        public void GetWhoHasClaims()
        {
            From<UserClaim> from = _db.From<UserClaim>().LeftJoin<User>((uc, u) => uc.UserId == u.Id);


            var resultList = _db.RetrieveAsync<UserClaim, User>(from, null).Result;

            foreach (var item in resultList)
            {
                _output.WriteLine(DataConverter.ToJson(item));
            }

            Assert.NotEmpty(resultList);
        }

        [Fact]
        public void RandomUpdateSomeUserAsync()
        {
            DbTransactionContext transContext = BeginTransaction<User>();

            string userNamePrefix = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

            List<long> ids = new List<long>();

            DatabaseResult result = null;

            try
            {
                long userCount = _db.Count<User>(transContext);

                for (int i = 0; i < userCount; i += 10)
                {
                    int userId = i;

                    User user = _db.ScalarAsync<User>(i, transContext).Result;

                    if (user == null)
                    {
                        continue;
                    }

                    ids.Add(userId);

                    user.UserName = userNamePrefix + "_update_" + i;

                    result = _db.UpdateAsync<User>(user, transContext).Result;

                    Assert.True(result.IsSucceeded());

                    if (!result.IsSucceeded())
                    {
                        _output.WriteLine($"user id {userId} update failed.");

                        Rollback(transContext);
                        break;
                    }
                }

                Commit(transContext);

                Assert.Equal(DbTransactionStatus.Commited, transContext.Status);

                if (transContext.Status == DbTransactionStatus.Commited)
                {
                    IList<User> updatedUsers = _db.Retrieve<User>(u => SQLUtility.In(u.Id, ids));

                    foreach (User u in updatedUsers)
                    {
                        _output.WriteLine(u.UserName);
                    }
                }
            }
            catch (Exception ex)
            {
           
                Rollback(transContext);

                result = DatabaseResult.Failed();
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void RandomDeleteSomeUserAsync()
        {
            DbTransactionContext transContext = BeginTransaction<User>();

            List<long> ids = new List<long>();

            DatabaseResult result = null;

            try
            {
                long userCount = _db.Count<User>(transContext);

                for (int i = 0; i < userCount; i += 10)
                {
                    int userId = i;

                    User user = _db.ScalarAsync<User>(i, transContext).Result;

                    if (user == null)
                    {
                        continue;
                    }

                    ids.Add(userId);

                    result = _db.DeleteAsync<User>(user, transContext).Result;

                    Assert.True(result.IsSucceeded());

                    if (!result.IsSucceeded())
                    {

                        Rollback(transContext);
                        break;
                    }
                }

                Commit(transContext);

                Assert.True(transContext.Status == DbTransactionStatus.Commited);

                if (transContext.Status == DbTransactionStatus.Commited)
                {
                    IList<User> updatedUsers = _db.Retrieve<User>(u => SQLUtility.In(u.Id, ids));

                    if (updatedUsers.Count == 0)
                    {
                        result = DatabaseResult.Succeeded();
                    }

                    result = DatabaseResult.Failed();
                }

                result = DatabaseResult.Failed();
            }
            catch (Exception)
            {
                Rollback(transContext);
            }
        }

        [Fact]
        public void GetWhoHasRoles()
        {
            From<UserRole> from = _db.From<UserRole>().LeftJoin<Role>((ur, r) => ur.RoleId == r.Id).LeftJoin<User>((ur, u) => ur.UserId == u.Id);

            var resultList = _db.RetrieveAsync<UserRole, User, Role>(from, null).Result;

            foreach (var item in resultList)
            {
                _output.WriteLine(DataConverter.ToJson(item));
            }

            Assert.NotEmpty(resultList);
        }
    }
}
