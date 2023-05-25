using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

using HB.FullStack.BaseTest.DapperMapper;
using HB.FullStack.BaseTest.Models;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.MySqlTests;

using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MySqlConnector;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class BasicTest_Guid : BaseTestClass
    {
        #region Key Conflict

        private async Task Test_Add_Key_Conflict_ErrorAsync_Core<T>() where T : class, IDbModel
        {
            IDbModel dbModel = Mocker.Mock<T>(1).First();

            await Db.AddAsync(dbModel, "", null);

            try
            {
                await Db.AddAsync(dbModel, "", null);
            }
            catch (DbException e)
            {
                Assert.IsTrue(e.ErrorCode == ErrorCodes.DuplicateKeyEntry);
            }
        }

        [TestMethod]
        public async Task Test_Add_Key_Conflict_ErrorAsync()
        {
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Add_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();

        }

        private async Task Test_BatchAdd_Key_Conflict_ErrorAsync_Core<T>() where T : class, IDbModel
        {
            IList<T> dbModels = Mocker.Mock<T>(50);

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(dbModels, "tester", trans);

                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }

            TransactionContext trans2 = await Trans.BeginTransactionAsync<T>();

            try
            {
                await Db.AddAsync(dbModels, "tester", trans2);

                await trans2.CommitAsync();
            }
            catch (DbException e)
            {
                Assert.IsTrue(e.ErrorCode == ErrorCodes.DuplicateKeyEntry);

                await trans2.RollbackAsync();
            }
        }

        [TestMethod]
        public async Task Test_BatchAdd_Key_Conflict_ErrorAsync()
        {
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_BatchAdd_Key_Conflict_ErrorAsync_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region Concurrency Conflict

        private async Task Test_UpdateProperties_UsingTimestamp_Core<T>() where T : class, IBookModel, ITimestamp
        {
            //Add
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            TimestampUpdatePack updatePack = new TimestampUpdatePack
            {
                Id = book.Id,
                OldTimestamp = (book as ITimestamp)!.Timestamp,
                PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
            };

            await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);

            IBookModel? updatedBook = await Db.ScalarAsync<T>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            Assert.IsTrue((updatedBook as ITimestamp)!.Timestamp > book.Timestamp);

            //应该抛出冲突异常
            try
            {
                await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);
            }
            catch (DbException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }
        }

        [TestMethod]
        public async Task Test_UpdateProperties_UsingTimestamp()
        {
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_UpdateProperties_UsingTimestamp_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_Update_Properties_UsingOldNewCompare_Core<T>() where T : IBookModel
        {
            //Add
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            //update-fields

            OldNewCompareUpdatePack updatePack = new OldNewCompareUpdatePack
            {
                Id = book.Id,
                PropertyNames = new string[] { nameof(IBookModel.Price), nameof(IBookModel.Name) },
                OldPropertyValues = new object?[] { book.Price, book.Name },
                NewPropertyValues = new object?[] { 123456.789, "TTTTTXXXXTTTTT" }
            };

            await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);

            var updatedBook = await Db.ScalarAsync<T>(book.Id, null);

            Assert.IsNotNull(updatedBook);

            Assert.IsTrue(updatedBook.Price == 123456.789);
            Assert.IsTrue(updatedBook.Name == "TTTTTXXXXTTTTT");
            Assert.IsTrue(updatedBook.LastUser == "UPDATE_FIELDS_VERSION");
            //Assert.IsTrue(updatedBook.Timestamp > book.Timestamp);

            //应该抛出冲突异常
            try
            {
                await Db.UpdatePropertiesAsync<T>(updatePack, "UPDATE_FIELDS_VERSION", null);
            }
            catch (DbException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw ex;
                }
            }
        }

        [TestMethod]
        public async Task Test_Update_Properties_UsingOldNewCompare()
        {
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timeless_Long_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Update_Properties_UsingOldNewCompare_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();
        }

        private async Task Test_ConcurrencyConflict_Error_Core<T>() where T : IPublisherModel
        {
            var publisher = Mocker.Mock<T>(1).First();

            await Db.AddAsync(publisher, "tester", null);

            var publisher1 = await Db.ScalarAsync<T>(publisher.Id, null);
            var publisher2 = await Db.ScalarAsync<T>(publisher.Id, null);

            //update book1
            publisher1!.Name = "Update Book1";
            await Db.UpdateAsync(publisher1, "test", null);

            //update book2
            try
            {
                publisher2!.Name = "Update book2";
                await Db.UpdateAsync(publisher2, "tester", null);
            }
            catch (DbException ex)
            {
                Assert.IsTrue(ex.ErrorCode == ErrorCodes.ConcurrencyConflict);

                if (ex.ErrorCode != ErrorCodes.ConcurrencyConflict)
                {
                    throw;
                }
            }

            var publisher3 = await Db.ScalarAsync<T>(publisher.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(publisher1) == SerializeUtil.ToJson(publisher3));
        }

        [TestMethod]
        public async Task Test_ConcurrencyConflict_Error()
        {
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region Engine

        private async Task Test_Repeate_Update_Return_1_Core<T>() where T : IDbModel
        {
            var dbModel = Mocker.Mock<T>(1).First();

            await Db.AddAsync(dbModel, "tester", null);


            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            var parameters = new List<KeyValuePair<string, object>>()
                .AddParameter(modelDef.PrimaryKeyPropertyDef, modelDef.PrimaryKeyPropertyDef.GetValueFrom(dbModel), null, 0);

            var command = new DbEngineCommand(
                $"update {modelDef.DbTableReservedName} set LastUser ='Update_xxx' where Id = {parameters[0].Key}",
                parameters);

            int rt = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);
            int rt2 = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);
            int rt3 = await modelDef.Engine.ExecuteCommandNonQueryAsync(modelDef.MasterConnectionString, command);

            Assert.IsTrue(1 == rt);

            Assert.IsTrue(1 == rt2);

            Assert.IsTrue(1 == rt3);
        }

        /// <summary>
        /// //NOTICE: 在sqlite下，重复update，返回1.即matched
        /// //NOTICE: 在mysql下，重复update，返回1，即mactched
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Repeate_Update_Return_1()
        {
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_ConcurrencyConflict_Error_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }



        private async Task Test_Mult_SQL_Return_With_Reader_Core<T>() where T : IBookModel
        {
            var book = Mocker.Mock<T>(1).First();

            await Db.AddAsync(book, "tester", null);

            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            var parameters = new List<KeyValuePair<string, object>>().AddParameter(modelDef.PrimaryKeyPropertyDef, modelDef.PrimaryKeyPropertyDef.GetValueFrom(book), null, 0);

            string sql = @$"
update {modelDef.DbTableReservedName} set LastUser='TTTgdTTTEEST' where Id = {parameters[0].Key};
select count(1) from {modelDef.DbTableReservedName} where Id = {parameters[0].Key};
";
            var command = new DbEngineCommand(sql, parameters);

            using IDataReader reader = await modelDef.Engine.ExecuteCommandReaderAsync(modelDef.MasterConnectionString, command);

            List<string?> rt = new List<string?>();

            while (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rt.Add(reader.GetValue(i)?.ToString());
                }
            }

            Assert.AreEqual(rt.Count, 1);
            Assert.AreNotEqual(rt.Count, 1);
        }


        /// <summary>
        /// //NOTICE: Mysql执行多条语句的时候，ExecuteCommandReader只返回最后一个结果。
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task Test_Mult_SQL_Return_With_Reader()
        {
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Mult_SQL_Return_With_Reader_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();
        }

        #endregion

        #region Add

        private async Task Test_Add_Core<T>() where T : IDbModel
        {
            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                IList<T> lst = new List<T>();

                for (int i = 0; i < 10; ++i)
                {
                    T model = Mocker.MockOne<T>();

                    await Db.AddAsync(model, "lastUsre", trans).ConfigureAwait(false);

                    lst.Add(model);
                }

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rt = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, lst.Select(s => s.Id)), null);

                Assert.IsTrue(SerializeUtil.ToJson(lst) == SerializeUtil.ToJson(rt));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Add()
        {
            await Test_Add_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Add_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Add_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Add_Core<MySql_Timeless_Long_BookModel>();
            await Test_Add_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Add_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Add_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Add_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Add_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_Add_Core<T>() where T : IDbModel
        {
            var publishers = Mocker.Mock<T>(50);

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await transactionContext.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await transactionContext.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Batch_Add()
        {
            await Test_Batch_Add_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Add_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Add_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region Update

        private async Task Test_Update_Core<T>() where T : IDbModel
        {
            var models = Mocker.Mock<T>(50);

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(models, "", trans);

                foreach (var model in models)
                {
                    Mocker.Modify(model);

                    await Db.UpdateAsync(model, "", trans);
                }

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), null);

                Assert.IsTrue(SerializeUtil.ToJson(models) == SerializeUtil.ToJson(rts));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Update()
        {
            await Test_Update_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Update_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Update_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Update_Core<MySql_Timeless_Long_BookModel>();
            await Test_Update_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Update_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Update_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Update_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Update_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Batch_Update_Core<T>() where T : IDbModel
        {
            var dbModels = Mocker.Mock<T>(50);

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(dbModels, "", trans);

                var rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, dbModels.Select(d => d.Id)), trans);

                Mocker.Modify(rts);

                await Db.UpdateAsync(rts, "lastUsre", trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts2 = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, dbModels.Select(d => d.Id)), null);

                Assert.IsTrue(SerializeUtil.ToJson(rts) == SerializeUtil.ToJson(rts2));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Batch_Update()
        {
            await Test_Batch_Update_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Update_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Update_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Update_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region Delete

        private async Task Test_Batch_Delete_Core<T>() where T : IDbModel
        {
            var models = Mocker.Mock<T>(50);

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(models, "", trans);

                var rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), trans);

                Assert.IsTrue(rts.Count == models.Count);

                await Db.DeleteAsync(rts, "Delete", trans);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                var rts2 = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), null);

                Assert.IsTrue(rts2.Count == 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Batch_Delete_Core()
        {
            await Test_Batch_Delete_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_BookModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Batch_Delete_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Batch_Delete_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Batch_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion



        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_06_Delete_PublisherModelAsync(DbEngineType engineType)
        {
            TransactionContext tContext = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                IList<Timestamp_Guid_PublisherModel> testModels = (await Db.RetrieveAllAsync<Timestamp_Guid_PublisherModel>(tContext).ConfigureAwait(false)).ToList();

                foreach (var model in testModels)
                {
                    await Db.DeleteAsync(model, "lastUsre", tContext).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<Timestamp_Guid_PublisherModel>(tContext).ConfigureAwait(false);

                await Trans.CommitAsync(tContext).ConfigureAwait(false);

                Assert.IsTrue(count == 0);
            }
            catch (Exception ex)
            {
                await Trans.RollbackAsync(tContext).ConfigureAwait(false);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_08_LastTimeTestAsync(DbEngineType engineType)
        {
            Timestamp_Guid_PublisherModel item = Mocker.Guid_MockOnePublisherModel();

            await Db.AddAsync(item, "xx", null).ConfigureAwait(false);

            var fetched = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

            fetched.Name = "ssssss";

            await Db.UpdateAsync(fetched, "xxx", null).ConfigureAwait(false);

            fetched = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            //await Db.AddOrUpdateAsync(item, "ss", null);

            fetched = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(item.Id, null).ConfigureAwait(false);

            //Batch

            var items = Mocker.Guid_GetPublishers();

            TransactionContext trans = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(items, "xx", trans).ConfigureAwait(false);

                var results = await Db.RetrieveAsync<Timestamp_Guid_PublisherModel>(item => SqlStatement.In(item.Id, true, items.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.UpdateAsync(items, "xx", trans).ConfigureAwait(false);

                var items2 = Mocker.Guid_GetPublishers();

                await Db.AddAsync(items2, "xx", trans).ConfigureAwait(false);

                results = await Db.RetrieveAsync<Timestamp_Guid_PublisherModel>(item => SqlStatement.In(item.Id, true, items2.Select(item => (object)item.Id).ToArray()), trans).ConfigureAwait(false);

                await Db.UpdateAsync(items2, "xx", trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                throw;
            }
            finally
            {
            }
        }

        /// <summary>
        /// Test_9_UpdateLastTimeTestAsync
        /// </summary>
        /// <returns></returns>
        /// <exception cref="DbException">Ignore.</exception>
        /// <exception cref="Exception">Ignore.</exception>
        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_09_UpdateLastTimeTestAsync(DbEngineType engineType)
        {
            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                Timestamp_Guid_PublisherModel item = Mocker.Guid_MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                IList<Timestamp_Guid_PublisherModel> testModels = (await Db.RetrieveAllAsync<Timestamp_Guid_PublisherModel>(transactionContext, 0, 1).ConfigureAwait(false)).ToList();

                if (testModels.Count == 0)
                {
                    throw new Exception("No Model to update");
                }

                Timestamp_Guid_PublisherModel model = testModels[0];

                model.Books.Add("New Book2");
                //model.BookAuthors.Add("New Book2", new Author() { Mobile = "15190208956", Code = "Yuzhaobai" });

                await Db.UpdateAsync(model, "lastUsre", transactionContext).ConfigureAwait(false);

                Timestamp_Guid_PublisherModel? stored = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(model.Id, transactionContext).ConfigureAwait(false);

                item = Mocker.Guid_MockOnePublisherModel();

                await Db.AddAsync(item, "xx", transactionContext).ConfigureAwait(false);

                var fetched = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(item.Id, transactionContext).ConfigureAwait(false);

                Assert.AreEqual(item.Timestamp, fetched!.Timestamp);

                fetched.Name = "ssssss";

                await Db.UpdateAsync(fetched, "xxx", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_10_Enum_TestAsync(DbEngineType engineType)
        {
            IList<Timestamp_Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Timestamp_Guid_PublisherModel> publisherModels = await Db.RetrieveAsync<Timestamp_Guid_PublisherModel>(p => p.Type == PublisherType.Big && p.LastUser == "lastUsre", null).ConfigureAwait(false);

            Assert.IsTrue(publisherModels.Any() && publisherModels.All(p => p.Type == PublisherType.Big));
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_11_StartWith_TestAsync(DbEngineType engineType)
        {
            IList<Timestamp_Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            foreach (var model in publishers)
            {
                model.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Timestamp_Guid_PublisherModel> models = await Db.RetrieveAsync<Timestamp_Guid_PublisherModel>(t => t.Name.StartsWith("Star"), null);

            Assert.IsTrue(models.Any());

            Assert.IsTrue(models.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_12_Binary_TestAsync(DbEngineType engineType)
        {
            IList<Timestamp_Guid_PublisherModel> publishers = Mocker.Guid_GetPublishers();

            foreach (var model in publishers)
            {
                model.Name = "StartWithTest_xxx";
            }

            TransactionContext transactionContext = await Trans.BeginTransactionAsync<Timestamp_Guid_PublisherModel>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(publishers, "lastUsre", transactionContext).ConfigureAwait(false);

                await Trans.CommitAsync(transactionContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Trans.RollbackAsync(transactionContext).ConfigureAwait(false);
                throw;
            }

            IEnumerable<Timestamp_Guid_PublisherModel> models = await Db.RetrieveAsync<Timestamp_Guid_PublisherModel>(
                t => t.Name.StartsWith("Star") && publishers.Any(), null);

            //IEnumerable<Guid_PublisherModel> models = await Db.RetrieveAsync<Guid_PublisherModel>(
            //    t => ReturnGuid() == ReturnGuid(), null);

            Assert.IsTrue(models.Any());

            Assert.IsTrue(models.All(t => t.Name.StartsWith("Star")));
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_13_Mapper_ToModelAsync(DbEngineType engineType)
        {
            Globals.Logger.LogDebug($"��ǰProcess,{Environment.ProcessId}");

            #region Json验证1

            var publisher3 = new Timestamp_Guid_PublisherModel();

            await Db.AddAsync(publisher3, "sss", null).ConfigureAwait(false);

            var stored3 = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(publisher3.Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher3), SerializeUtil.ToJson(stored3));

            #endregion

            #region Json验证2

            var publisher2s = Mocker.Guid_GetPublishers2();

            foreach (Guid_PublisherModel2 publisher in publisher2s)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Guid_PublisherModel2? publisher2 = await Db.ScalarAsync<Guid_PublisherModel2>(publisher2s[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher2), SerializeUtil.ToJson(publisher2s[0]));

            #endregion

            #region Json验证3

            var publishers = Mocker.Guid_GetPublishers();

            foreach (Timestamp_Guid_PublisherModel publisher in publishers)
            {
                await Db.AddAsync(publisher, "yuzhaobai", null).ConfigureAwait(false);
            }

            Timestamp_Guid_PublisherModel? publisher1 = await Db.ScalarAsync<Timestamp_Guid_PublisherModel>(publishers[0].Id, null).ConfigureAwait(false);

            Assert.AreEqual(SerializeUtil.ToJson(publisher1), SerializeUtil.ToJson(publishers[0]));

            #endregion
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public async Task Guid_Test_14_Mapper_ToModel_PerformanceAsync(DbEngineType engineType)
        {
            var books = GetBooks(50);

            var trans = await Trans.BeginTransactionAsync<Guid_Timestamp_BookModel>().ConfigureAwait(false);

            try
            {
                await Db.AddAsync(books, "x", trans).ConfigureAwait(false);
                await Trans.CommitAsync(trans).ConfigureAwait(false);
            }
            catch
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
            }

            Stopwatch stopwatch = new Stopwatch();

            using MySqlConnection mySqlConnection = new MySqlConnection(DbConfigManager.GetRequiredConnectionString(DbSchema_Mysql, true).ToString());

            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 1;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                await mySqlConnection.OpenAsync().ConfigureAwait(false);

                using MySqlCommand command0 = new MySqlCommand("select * from tb_Guid_Book limit 5000", mySqlConnection);

                var reader = await command0.ExecuteReaderAsync().ConfigureAwait(false);

                List<Guid_Timestamp_BookModel> list1 = new List<Guid_Timestamp_BookModel>();
                List<Guid_Timestamp_BookModel> list2 = new List<Guid_Timestamp_BookModel>();
                List<Guid_Timestamp_BookModel> list3 = new List<Guid_Timestamp_BookModel>();

                int len = reader.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                DbModelDef definition = Db.ModelDefFactory.GetDef<Guid_Timestamp_BookModel>()!;

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = definition.GetDbPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> fullStack_mapper = DbModelConvert.CreateDataReaderRowToModelDelegate(definition, reader, 0, definition.FieldCount, false);

                Func<IDataReader, object> dapper_mapper = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(Guid_Timestamp_BookModel), reader);

                Func<IDataReader, object> reflection_mapper = (r) =>
                {
                    Guid_Timestamp_BookModel item = new Guid_Timestamp_BookModel();

                    for (int i = 0; i < len; ++i)
                    {
                        DbModelPropertyDef property = propertyDefs[i];

                        object? value = DbPropertyConvert.DbFieldValueToPropertyValue(r[i], property, DbEngineType.MySQL);

                        if (value != null)
                        {
                            setMethods[i].Invoke(item, new object?[] { value });
                        }
                    }

                    return item;
                };

                Stopwatch stopwatch1 = new Stopwatch();
                Stopwatch stopwatch2 = new Stopwatch();
                Stopwatch stopwatch3 = new Stopwatch();

                while (reader.Read())
                {
                    stopwatch1.Start();
                    object obj1 = fullStack_mapper(Db.ModelDefFactory, reader);
                    list1.Add((Guid_Timestamp_BookModel)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = dapper_mapper(reader);
                    list2.Add((Guid_Timestamp_BookModel)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();
                    object obj3 = reflection_mapper(reader);
                    list3.Add((Guid_Timestamp_BookModel)obj3);
                    stopwatch3.Stop();
                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;

                await reader.DisposeAsync().ConfigureAwait(false);
                command0.Dispose();

                await mySqlConnection.CloseAsync().ConfigureAwait(false);
            }

            Console.WriteLine("FullStack_Emit : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("FullStack_Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public void Guid_Test_15_Mapper_ToParameter(DbEngineType engineType)
        {
            Timestamp_Guid_PublisherModel publisherModel = Mocker.Guid_MockOnePublisherModel();
            //publisherModel.Version = 0;

            var emit_results = publisherModel.ToDbParameters(Db.ModelDefFactory.GetDef<Timestamp_Guid_PublisherModel>()!, Db.ModelDefFactory, 1);

            var reflect_results = publisherModel.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Timestamp_Guid_PublisherModel>()!, 1);

            AssertEqual(emit_results, reflect_results, DbEngineType.MySQL);

            //PublisherModel2

            Guid_PublisherModel2 publisherModel2 = new Guid_PublisherModel2();

            IList<KeyValuePair<string, object>>? emit_results2 = publisherModel2.ToDbParameters(Db.ModelDefFactory.GetDef<Guid_PublisherModel2>()!, Db.ModelDefFactory, 1);

            var reflect_results2 = publisherModel2.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Guid_PublisherModel2>()!, 1);

            AssertEqual(emit_results2, reflect_results2, DbEngineType.MySQL);

            //PublisherModel3

            Guid_PublisherModel3 publisherModel3 = new Guid_PublisherModel3();

            var emit_results3 = publisherModel3.ToDbParameters(Db.ModelDefFactory.GetDef<Guid_PublisherModel3>()!, Db.ModelDefFactory, 1);

            var reflect_results3 = publisherModel3.ToDbParametersUsingReflection(Db.ModelDefFactory.GetDef<Guid_PublisherModel3>()!, 1);

            AssertEqual(emit_results3, reflect_results3, DbEngineType.MySQL);
        }

        [TestMethod]
        [DataRow(DbEngineType.MySQL)]
        [DataRow(DbEngineType.SQLite)]
        public void Guid_Test_16_Mapper_ToParameter_Performance(DbEngineType engineType)
        {
            var models = Mocker.Guid_GetPublishers(1000000);

            var def = Db.ModelDefFactory.GetDef<Timestamp_Guid_PublisherModel>();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParameters(def!, Db.ModelDefFactory);
            }
            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParametersUsingReflection(def!);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }

        [TestMethod]
        public async Task Test_FieldLength_OversizeAsync()
        {
            //TODO: 测试指定字段长度为10，结果赋值字符串长度为100，怎么处理

            FieldLengthTestModel model = new FieldLengthTestModel { Content = "12345678910" };

            var ex = await Assert.ThrowsExceptionAsync<DbException>(() => Db.AddAsync(model, "", null));

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.DbDataTooLong);
        }

        private static void AssertEqual(IEnumerable<KeyValuePair<string, object>> emit_results, IEnumerable<KeyValuePair<string, object>> results, DbEngineType engineType)
        {
            var dict = results.ToDictionary(kv => kv.Key);

            Assert.IsTrue(emit_results.Count() == dict.Count);

            foreach (var kv in emit_results)
            {
                Assert.IsTrue(dict.ContainsKey(kv.Key));

                Assert.IsTrue(DbPropertyConvert.DoNotUseUnSafePropertyValueToDbFieldValueStatement(dict[kv.Key].Value, false, engineType) ==

                    DbPropertyConvert.DoNotUseUnSafePropertyValueToDbFieldValueStatement(kv.Value, false, engineType));
            }
        }
    }
}