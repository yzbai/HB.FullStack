using HB.Framework.Database;
using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using Xunit;
using Xunit.Abstractions;

namespace HB.Infrastructure.SQLite.Test
{
    [TestCaseOrderer("HB.Infrastructure.SQLite.Test.TestCaseOrdererByTestName", "HB.Infrastructure.SQLite.Test")]
    public class BasicTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase database;
        private readonly ITestOutputHelper output;
        private readonly IsolationLevel isolationLevel = IsolationLevel.Serializable;


        public BasicTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            output = testOutputHelper;
            database = serviceFixture.Database;
            database.Initialize();
        }

        [Fact]
        public void Test_1_Batch_Add_PublisherEntity()
        {
            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            var transactionContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            DatabaseResult result = DatabaseResult.Failed();
            try
            {
                result = database.BatchAdd<PublisherEntity>(publishers, "tester", transactionContext);

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    throw new Exception();
                }

                database.Commit(transactionContext);

            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                database.Rollback(transactionContext);
                throw ex;
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public void Test_2_Batch_Update_PublisherEntity()
        {
            TransactionContext transContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> lst = database.RetrieveAll<PublisherEntity>(transContext);

                for (int i = 0; i < lst.Count; i += 2)
                {
                    PublisherEntity entity = lst[i];
                    //entity.Guid = Guid.NewGuid().ToString();
                    entity.Type = PublisherType.Online;
                    entity.Name = "ÖÐsfasfafÎÄÃû×Ö";
                    entity.Books = new List<string>() { "xxx", "tttt" };
                    entity.BookAuthors = new Dictionary<string, Author>()
                    {
                        { "Cat", new Author() { Mobile="111", Name="BB" } },
                        { "Dog", new Author() { Mobile="222", Name="sx" } }
                    };
                }

                DatabaseResult result = database.BatchUpdate<PublisherEntity>(lst, "tester", transContext);

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    database.Rollback(transContext);
                    throw new Exception();
                }

                database.Commit(transContext);

                Assert.True(result.IsSucceeded());
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                database.Rollback(transContext);
                throw ex;
            }
        }

        [Fact]
        public void Test_3_Batch_Delete_PublisherEntity()
        {
            TransactionContext transactionContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> lst = database.Page<PublisherEntity>(2, 100, transactionContext);

                if (lst.Count != 0)
                {
                    DatabaseResult result = database.BatchDelete<PublisherEntity>(lst, "deleter", transactionContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());

                }

                database.Commit(transactionContext);
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                database.Rollback(transactionContext);
                throw ex;
            }
        }

        [Fact]
        public void Test_4_Add_PublisherEntity()
        {
            TransactionContext tContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity entity = Mocker.MockOne();

                    DatabaseResult result = database.Add(entity, tContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());
                }

                database.Commit(tContext);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);
                database.Rollback(tContext);
                throw ex;
            }
        }

        [Fact]
        public void Test_5_Update_PublisherEntity()
        {
            var tContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> testEntities = database.Page<PublisherEntity>(1, 1, tContext);

                if (testEntities.Count == 0)
                {
                    return;
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book");
                entity.BookAuthors.Add("New Book", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                DatabaseResult result = database.Update(entity, tContext);

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    throw new Exception();
                }

                Assert.True(result.IsSucceeded());

                PublisherEntity stored = database.Scalar<PublisherEntity>(entity.Id, tContext);

                Assert.True(stored.Books.Contains("New Book"));
                Assert.True(stored.BookAuthors["New Book"].Mobile == "15190208956");

                database.Commit(tContext);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);
                database.Rollback(tContext);
                throw ex;
            }
        }

        [Fact]
        public void Test_6_Delete_PublisherEntity()
        {
            var tContext = database.BeginTransaction<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> testEntities = database.RetrieveAll<PublisherEntity>(tContext);

                testEntities.ForEach(entity => {
                    DatabaseResult result = database.Delete(entity, tContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());
                });

                long count = database.Count<PublisherEntity>(tContext);

                database.Commit(tContext);

                output.WriteLine($"count: {count}");

                Assert.True(count == 0);
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
