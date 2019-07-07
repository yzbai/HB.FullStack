using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace HB.Framework.Database.Test
{
    public class ComplexFieldTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase database;
        private readonly ITestOutputHelper output;


        public ComplexFieldTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            output = testOutputHelper;
            database = serviceFixture.Database;

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

            var transactionContext = database.BeginTransaction<TestEntity>();

            DatabaseResult result = DatabaseResult.Failed();
            try
            {
                result = database.BatchAdd<TestEntity>(lst, "tester", transactionContext);

                if (!result.IsSucceeded())
                {
                    throw new Exception();
                }

                database.Commit(transactionContext);

            }
            catch (Exception ex)
            {
                database.Rollback(transactionContext);
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Batch_Update_TestEntity()
        {

            IList<TestEntity> lst = database.RetrieveAll<TestEntity>(null);

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

            TransactionContext transContext = database.BeginTransaction<TestEntity>();
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
            IList<TestEntity> lst = database.RetrieveAll<TestEntity>(null);

            TransactionContext transactionContext = database.BeginTransaction<TestEntity>();

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

                DatabaseResult result = database.Add<TestEntity>(entity, null);

                Assert.True(result.IsSucceeded());

            }
        }

        [Fact]
        public void Test_Update_TestEntity()
        {
            IList<TestEntity> testEntities = database.RetrieveAll<TestEntity>(null);

            Assert.NotEmpty(testEntities);

            TestEntity entity = testEntities[0];

            entity.Books.Add("New Book");
            entity.BookAuthors.Add("New Book", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

            DatabaseResult result = database.Update(entity, null);

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_Delete_TestEntity()
        {
            IList<TestEntity> testEntities = database.RetrieveAll<TestEntity>(null);

            Assert.NotEmpty(testEntities);

            TestEntity entity = testEntities[0];

            DatabaseResult result = database.Delete(entity, null);

            Assert.True(result.IsSucceeded());
        }
    }
}
