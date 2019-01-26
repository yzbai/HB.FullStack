using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.Database.Test
{
    public class ComplexFieldTest : IClassFixture<ServiceFixture>
    {
        private IDatabase database;
        private ISQLBuilder sqlBuilder;
        private ITestOutputHelper output;

        public ComplexFieldTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            output = testOutputHelper;
            database = serviceFixture.Database;
            sqlBuilder = serviceFixture.SQLBuilder;
        }

        [Fact]
        public void Test_Create_Sql_For_TestEntity()
        {
            string sql = sqlBuilder.GetCreateStatement(typeof(TestEntity), true);

            output.WriteLine(sql);

            database.DatabaseEngine.ExecuteSqlNonQuery(null, "ahabit", sql);
        }

        [Fact]
        public void Test_Select_Sql_For_TestEntity()
        {
            IDbCommand command = sqlBuilder.CreateRetrieveCommand<TestEntity>();

            output.WriteLine(command.CommandText);

        }

        [Fact]
        public void Test_Retrieve_TestEntity()
        {
            IList<TestEntity> testEntities = database.RetrieveAll<TestEntity>();

            Assert.NotEmpty(testEntities);
            Assert.NotEmpty(testEntities[0].Books);
            Assert.NotEmpty(testEntities[0].BookAuthors);
        }

        [Fact]
        public void Test_Add_TestEntity()
        {
            TestEntity entity = new TestEntity();
            entity.Type = TestType.Hahaha;
            entity.Name = "EntityName";
            entity.Books = new List<string>() { "Cat", "Dog" };
            entity.BookAuthors = new Dictionary<string, Author>() 
            {
                { "Cat", new Author() { Mobile="111", Name="BB" } },
                { "Dog", new Author() { Mobile="222", Name="sx" } }
            };

            for (int i = 0; i < 100; ++i)
            {
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
            entity.BookAuthors.Add("New Book", new Author() { Mobile="15190208956", Name="Yuzhaobai" });

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
