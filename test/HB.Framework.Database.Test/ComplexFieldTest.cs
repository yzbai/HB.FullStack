using HB.Framework.Database.SQL;
using HB.Framework.Database.Transaction;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.Database.Test
{
    public class ComplexFieldTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase database;
        private readonly ISQLBuilder sqlBuilder;
        private readonly ITestOutputHelper output;

        public ComplexFieldTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            output = testOutputHelper;
            database = serviceFixture.Database;
            sqlBuilder = serviceFixture.SQLBuilder;
        }


        [Fact]
        public void Test_Batch_Add_TestEntity()
        {
            List<TestEntity> lst = new List<TestEntity>();

            for (int i = 0; i < 1000; i++)
            {
                TestEntity entity = new TestEntity();
                entity.Guid = Guid.NewGuid().ToString();
                entity.Type = TestType.Hahaha;
                entity.Name = "中文名字";
                entity.Books = new List<string>() { "Cat", "Dog" };
                entity.BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };

                lst.Add(entity);
            }

            var transaction = database.BeginTransaction<TestEntity>();

            DatabaseResult result = DatabaseResult.Failed();
            try
            {
                result = database.BatchAdd<TestEntity>(lst, "tester", transaction);

                if (!result.IsSucceeded())
                {
                    throw new Exception();
                }

                database.Commit(transaction);

            }
            catch (Exception ex)
            {
                database.Rollback(transaction);
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Batch_Update_TestEntity()
        {

            IList<TestEntity> lst = database.RetrieveAll<TestEntity>();

            for (int i = 0; i < lst.Count; i += 2)
            {
                TestEntity entity = lst[i];
                //entity.Guid = Guid.NewGuid().ToString();
                entity.Type = TestType.Hahaha;
                entity.Name = "中sfasfaf文名字";
                entity.Books = new List<string>() { "xxx", "tttt" };
                entity.BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };
            }

            DatabaseTransactionContext transContext = database.BeginTransaction<TestEntity>();
            DatabaseResult result = DatabaseResult.Failed();
            try
            {
                result = database.BatchUpdate<TestEntity>(lst, "tester", transContext);

                database.Commit(transContext);
            }
            catch (Exception ex)
            {
                database.Rollback(transContext);
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Batch_Delete_TestEntity()
        {
            IList<TestEntity> lst = database.RetrieveAll<TestEntity>();

            DatabaseTransactionContext transactionContext = database.BeginTransaction<TestEntity>();

            DatabaseResult result = DatabaseResult.Failed();

            try
            {
                result = database.BatchDelete<TestEntity>(lst, "deleter", transactionContext);

                database.Commit(transactionContext);
            }
            catch (Exception ex)
            {
                database.Rollback(transactionContext);
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Add_TestEntity()
        {
            for (int i = 0; i < 10; ++i)
            {
                TestEntity entity = new TestEntity();
                entity.Guid = Guid.NewGuid().ToString();
                entity.Type = TestType.Hahaha;
                entity.Name = "中文名字";
                entity.Books = new List<string>() { "Cat", "Dog" };
                entity.BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };

                DatabaseResult result = database.Add<TestEntity>(entity);

                Assert.True(result.IsSucceeded());

            }
        }

        [Fact]
        public void Test_Update_TestEntity()
        {
            IList<TestEntity> testEntities = database.RetrieveAll<TestEntity>();

            Assert.NotEmpty(testEntities);

            TestEntity entity = testEntities[0];

            entity.Books.Add("New Book");
            entity.BookAuthors.Add("New Book", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

            DatabaseResult result = database.Update(entity);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Delete_TestEntity()
        {
            IList<TestEntity> testEntities = database.RetrieveAll<TestEntity>();

            Assert.NotEmpty(testEntities);

            TestEntity entity = testEntities[0];

            DatabaseResult result = database.Delete(entity);

            Assert.True(result.IsSucceeded());
        }
    }
}
