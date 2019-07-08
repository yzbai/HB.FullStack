using HB.Framework.Database.SQL;
using HB.Framework.Database.Test.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.Database.Test
{
    [TestCaseOrderer("HB.Framework.Database.Test.TestCaseOrdererByTestName", "HB.Framework.Database.Test")]
    public class BasicAsyncTest : IClassFixture<ServiceFixture>
    {
        private readonly IDatabase database;
        private readonly ITestOutputHelper output;
        private readonly IsolationLevel isolationLevel = IsolationLevel.Serializable;

        public BasicAsyncTest(ITestOutputHelper testOutputHelper, ServiceFixture serviceFixture)
        {
            output = testOutputHelper;
            database = serviceFixture.Database;
            database.Initialize();
        }

        [Fact]
        public async Task Test_1_Batch_Add_PublisherEntityAsync()
        {
            IList<PublisherEntity> publishers = Mocker.GetPublishers();

            var transactionContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel).ConfigureAwait(false);

            DatabaseResult result = DatabaseResult.Failed();
            try
            {
                result = await database.BatchAddAsync<PublisherEntity>(publishers, "tester", transactionContext);

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    throw new Exception();
                }

                await database.CommitAsync(transactionContext);

            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                await database.RollbackAsync(transactionContext);
                throw ex;
            }

            Assert.True(result.IsSucceeded());
        }

        [Fact]
        public async Task Test_2_Batch_Update_PublisherEntityAsync()
        {
            TransactionContext transContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> lst = await database.RetrieveAllAsync<PublisherEntity>(transContext);

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

                DatabaseResult result = await database.BatchUpdateAsync<PublisherEntity>(lst, "tester", transContext);

                Assert.True(result.IsSucceeded());

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    await database.RollbackAsync(transContext);
                    throw new Exception();
                }

                await database.CommitAsync(transContext);
                
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                await database.RollbackAsync(transContext);
                throw ex;
            }
        }

        [Fact]
        public async Task Test_3_Batch_Delete_PublisherEntityAsync()
        {
            TransactionContext transactionContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> lst = await database.PageAsync<PublisherEntity>(2, 100, transactionContext);

                if (lst.Count != 0)
                {
                    DatabaseResult result = await database.BatchDeleteAsync<PublisherEntity>(lst, "deleter", transactionContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());
                }

                await database.CommitAsync(transactionContext);
            }
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                await database.RollbackAsync(transactionContext);
                throw ex;
            }
        }

        [Fact]
        public async Task Test_4_Add_PublisherEntityAsync()
        {
            var tContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel);

            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    PublisherEntity entity = Mocker.MockOne();

                    DatabaseResult result = await database.AddAsync(entity, tContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());
                }

                await database.CommitAsync(tContext);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);
                await database.RollbackAsync(tContext);
                throw ex;
            }
        }

        [Fact]
        public async Task Test_5_Update_PublisherEntityAsync()
        {
            var tContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> testEntities = await database.PageAsync<PublisherEntity>(1, 1, tContext);

                if (testEntities.Count == 0)
                {
                    return;
                }

                PublisherEntity entity = testEntities[0];

                entity.Books.Add("New Book2");
                entity.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Name = "Yuzhaobai" });

                DatabaseResult result = await database.UpdateAsync(entity, tContext);

                if (!result.IsSucceeded())
                {
                    output.WriteLine(result.Exception?.Message);
                    throw new Exception();
                }

                Assert.True(result.IsSucceeded());

                PublisherEntity stored = await database.ScalarAsync<PublisherEntity>(entity.Id, tContext);

                Assert.True(stored.Books.Contains("New Book2"));
                Assert.True(stored.BookAuthors["New Book2"].Mobile == "15190208956");

                await database.CommitAsync(tContext);
            }
            catch(Exception ex)
            {
                output.WriteLine(ex.Message);
                await database.RollbackAsync(tContext);
                throw ex;
            }
        }

        [Fact]
        public async Task Test_6_Delete_PublisherEntityAsync()
        {
            var tContext = await database.BeginTransactionAsync<PublisherEntity>(isolationLevel);

            try
            {
                IList<PublisherEntity> testEntities = await database.RetrieveAllAsync<PublisherEntity>(tContext);

                await testEntities.ForEachAsync(async entity => {
                    DatabaseResult result = await database.DeleteAsync(entity, tContext);

                    if (!result.IsSucceeded())
                    {
                        output.WriteLine(result.Exception?.Message);
                        throw new Exception();
                    }

                    Assert.True(result.IsSucceeded());
                });

                long count = await database.CountAsync<PublisherEntity>(tContext);

                Assert.True(count == 0);

                await database.CommitAsync(tContext);

                output.WriteLine($"count: {count}");
            }
            catch(Exception ex)
            {
                await database.RollbackAsync(tContext);
                output.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
