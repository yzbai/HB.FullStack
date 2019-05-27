using HB.Component.Identity.Entity;
using HB.Framework.Common;
using HB.Framework.Database;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace HB.Component.Identity.Test
{
    public class DatabaseTest : IClassFixture<ServiceFixture>
    {
        private ITestOutputHelper _output;
        private ServiceFixture _fixture;
        private IDatabase _db;
        private IDatabaseTransaction _dbTransaction;
        private IList<User> _userList;
        private readonly IList<Role> _roleList;

        public DatabaseTest(ITestOutputHelper output, ServiceFixture databaseTestFixture)
        {
            _output = output;
            _fixture = databaseTestFixture;
            _db = _fixture.Database;
            _dbTransaction = _fixture.DatabaseTransaction;

            _userList = DataMocker.MockUsers();
            _roleList = DataMocker.MockRoles();
            
        }

        [Fact]
        public void AddUsers()
        {
            DatabaseTransactionContext transContext = _dbTransaction.BeginTransaction<User>();
            DatabaseResult result = DatabaseResult.Failed();

            foreach (User item in _userList)
            {
                result = _db.AddAsync(item, transContext).Result;

                if (!result.IsSucceeded())
                {
                    _dbTransaction.Rollback(transContext);
                    break;
                }
            }

            _dbTransaction.Commit(transContext);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void AddRole()
        {
            DatabaseTransactionContext transContext = _dbTransaction.BeginTransaction<Role>();
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
                    _dbTransaction.Rollback(transContext);
                    break;
                }
            }

            _dbTransaction.Commit(transContext);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void AddSomeUserClaims()
        {
            int userCount = _userList.Count;
            DatabaseResult result = DatabaseResult.Failed();

            for (int i = 1; i < userCount; i += 39)
            {
                UserClaim uc = new UserClaim() { UserGuid = i.ToString(), ClaimValue = "Nothing", ClaimType = "HB.Nothing" };
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
                UserRole ur = new UserRole() { RoleGuid = roleId++.ToString(), UserGuid = i.ToString() };

                if (roleId == 5) { roleId = 1; }

                result = _db.AddAsync(ur).Result;
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void GetAdminUsers()
        {
            SelectExpression<User> select = _db.NewSelect<User>().Select(u => u.UserName).Select(u => u.Mobile);

            FromExpression<UserRole> from = _db.NewFrom<UserRole>()
                                          .LeftJoin<Role>((ur, r) => ur.RoleGuid == r.Guid)
                                          .LeftJoin<User>((ur, u) => ur.UserGuid == u.Guid);

            WhereExpression<UserRole> where = _db.NewWhere<UserRole>().And<Role>(r => r.Name == "Admin");

            IList<User> resultList = _db.RetrieveAsync(select, from, where).Result;

            if (resultList.Count > 0)
            {
                var roleList = _db.Retrieve<Role>(r => r.Name == "Admin");

                Assert.True(roleList.Count == 1);

                Role adminRole = roleList[0];

                foreach (User item in resultList)
                {
                    _output.WriteLine(JsonUtil.ToJson(item));

                    var lst = _db.Retrieve<UserRole>(ur => ur.UserGuid == item.Guid && ur.RoleGuid == adminRole.Guid);

                    Assert.True(lst.Count >= 1);
                }
            }
        }

        [Fact]
        public void GetWhoHasClaims()
        {
            FromExpression<UserClaim> from = _db.NewFrom<UserClaim>().LeftJoin<User>((uc, u) => uc.UserGuid == u.Guid);


            var resultList = _db.RetrieveAsync<UserClaim, User>(from, null).Result;

            foreach (var item in resultList)
            {
                _output.WriteLine(JsonUtil.ToJson(item));
            }

            Assert.NotEmpty(resultList);
        }

        [Fact]
        public void RandomUpdateSomeUserAsync()
        {
            DatabaseTransactionContext transContext = _dbTransaction.BeginTransaction<User>();

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

                        _dbTransaction.Rollback(transContext);
                        break;
                    }
                }

                _dbTransaction.Commit(transContext);

                Assert.Equal(DatabaseTransactionStatus.Commited, transContext.Status);

                if (transContext.Status == DatabaseTransactionStatus.Commited)
                {
                    IList<User> updatedUsers = _db.Retrieve<User>(u => SQLUtil.In(u.Id, ids));

                    foreach (User u in updatedUsers)
                    {
                        _output.WriteLine(u.UserName);
                    }
                }
            }
            catch (Exception)
            {

                _dbTransaction.Rollback(transContext);

                result = DatabaseResult.Failed();
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void RandomDeleteSomeUserAsync()
        {
            DatabaseTransactionContext transContext = _dbTransaction.BeginTransaction<User>();

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

                        _dbTransaction.Rollback(transContext);
                        break;
                    }
                }

                _dbTransaction.Commit(transContext);

                Assert.True(transContext.Status == DatabaseTransactionStatus.Commited);

                if (transContext.Status == DatabaseTransactionStatus.Commited)
                {
                    IList<User> updatedUsers = _db.Retrieve<User>(u => SQLUtil.In(u.Id, ids));

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
                _dbTransaction.Rollback(transContext);
            }
        }

        [Fact]
        public void GetWhoHasRoles()
        {
            FromExpression<UserRole> from = _db.NewFrom<UserRole>().LeftJoin<Role>((ur, r) => ur.RoleGuid == r.Guid).LeftJoin<User>((ur, u) => ur.UserGuid == u.Guid);

            var resultList = _db.RetrieveAsync<UserRole, User, Role>(from, null).Result;

            foreach (var item in resultList)
            {
                _output.WriteLine(JsonUtil.ToJson(item));
            }

            Assert.NotEmpty(resultList);
        }
    }
}
