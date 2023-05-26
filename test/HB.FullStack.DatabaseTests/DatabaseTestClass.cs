using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using HB.FullStack.BaseTest.DapperMapper;
using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.ClassLevel)]

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class DatabaseTestClass : BaseTestClass
    {
        #region Common

        internal async Task<IList<T>> AddAndRetrieve<T>(int count = 50, Action<int, T>? additionalAction = null) where T : IDbModel
        {
            var models = Mocker.Mock<T>(count, additionalAction);

            IList<T> rts;

            TransactionContext addTrans = await Trans.BeginTransactionAsync<T>();

            try
            {
                await Db.AddAsync(models, "", addTrans);

                rts = await Db.RetrieveAsync<T>(m => SqlStatement.In(m.Id, true, models.Select(s => s.Id)), addTrans);

                await addTrans.CommitAsync();
            }
            catch
            {
                await addTrans.RollbackAsync();
                throw;
            }

            Assert.IsTrue(SerializeUtil.ToJson(models) == SerializeUtil.ToJson(rts));

            return rts;
        }

        #endregion

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

        #region Add

        private async Task Test_Add_Core<T>() where T : IDbModel
        {
            T model = Mocker.MockOne<T>();

            await Db.AddAsync(model, "lastUsre", null).ConfigureAwait(false);

            var rt = await Db.ScalarAsync<T>(model.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));
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
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
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
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                await Db.DeleteAsync(models, "Delete", trans);

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



        private async Task Test_Delete_Core<T>() where T : IDbModel
        {
            var models = await AddAndRetrieve<T>();

            TransactionContext trans = await Trans.BeginTransactionAsync<T>().ConfigureAwait(false);

            try
            {
                foreach (var model in models)
                {
                    await Db.DeleteAsync(model, "lastUsre", trans).ConfigureAwait(false);
                }

                long count = await Db.CountAsync<T>(m => SqlStatement.In(m.Id, false, models.Select(s => s.Id)), trans).ConfigureAwait(false);

                await Trans.CommitAsync(trans).ConfigureAwait(false);

                Assert.IsTrue(count == 0);
            }
            catch (Exception ex)
            {
                await Trans.RollbackAsync(trans).ConfigureAwait(false);
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        [TestMethod]
        public async Task Test_Delete()
        {
            await Test_Delete_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Long_BookModel>();
            await Test_Delete_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Delete_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Delete_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Delete_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Delete_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region SQL Expression / Fields

        private async Task Test_FieldLength_Oversize_Core<T>() where T : IPublisherModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;

            var propertyDef = modelDef.GetDbPropertyDef(nameof(IPublisherModel.Name2))!;

            if (!propertyDef.IsLengthFixed)
            {
                throw new Exception("Not Length Fixed");
            }

            var model = Mocker.MockOne<T>(t => { t.Name2 = SecurityUtil.CreateRandomString(propertyDef.DbMaxLength!.Value + 1); });

            //TODO: 测试指定字段长度为10，结果赋值字符串长度为100，怎么处理

            var ex = await Assert.ThrowsExceptionAsync<DbException>(() => Db.AddAsync(model, "", null));

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.DbDataTooLong);
        }

        [TestMethod]
        public async Task Test_FieldLength_Oversize()
        {
            await Test_FieldLength_Oversize_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_FieldLength_Oversize_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_FieldLength_Oversize_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_FieldLength_Oversize_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_FieldLength_Oversize_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_FieldLength_Oversize_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_FieldLength_Oversize_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_Enum_Core<T>() where T : IPublisherModel
        {
            var models = await AddAndRetrieve<T>();

            IList<T> rts = await Db.RetrieveAsync<T>(p => p.Type == PublisherType.Big, null).ConfigureAwait(false);

            Assert.IsTrue(rts.Any() && rts.All(p => p.Type == PublisherType.Big));
        }

        [TestMethod]
        public async Task Test_Enum()
        {
            await Test_Enum_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Enum_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Enum_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Enum_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Enum_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Enum_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Enum_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private async Task Test_StartWith_Core<T>() where T : IPublisherModel
        {
            //Clear
            var trans = await Trans.BeginTransactionAsync<T>();

            try
            {
                await Db.DeleteAsync<T>(t => t.Name.StartsWith("StartWith_"), "", trans);

                await trans.CommitAsync();
            }
            catch
            {
                await trans.RollbackAsync();
                throw;
            }

            var models = await AddAndRetrieve<T>(50, (_, t) => { t.Name = $"StartWith_{SecurityUtil.CreateRandomString(4)}"; });

            var rts = await Db.RetrieveAsync<T>(t => t.Name.StartsWith("StarWith_"), null);

            string modelsJson = SerializeUtil.ToJson(models.OrderBy(m => m.Id).ToList());
            string rtsJson = SerializeUtil.ToJson(rts.OrderBy(models => models.Id).ToList());

            Assert.AreEqual(modelsJson, rtsJson);
        }


        [TestMethod]
        public async Task Test_StartWith()
        {
            await Test_StartWith_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_StartWith_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_StartWith_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_StartWith_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_StartWith_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_StartWith_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_StartWith_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        #endregion

        #region Convert

        private async Task Test_Mapper_ToModel_Performance_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            var books = await AddAndRetrieve<T>(50);

            //SetUp Dapper
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(DateTimeOffset), new DateTimeOffsetTypeHandler(), false);
            TypeHandlerHelper.AddTypeHandlerImpl(typeof(Guid), new MySqlGuidTypeHandler(), false);

            //time = 0;
            int loop = 10;

            TimeSpan time0 = TimeSpan.Zero, time1 = TimeSpan.Zero, time2 = TimeSpan.Zero, time3 = TimeSpan.Zero;
            for (int cur = 0; cur < loop; ++cur)
            {
                using var reader = await modelDef.Engine.ExecuteCommandReaderAsync(
                    modelDef.MasterConnectionString,
                    new DbEngineCommand("select * from {modelDef.DbTableReservedName} limit 5000")).ConfigureAwait(false);

                List<T> list1 = new List<T>();
                List<T> list2 = new List<T>();
                List<T> list3 = new List<T>();

                int len = reader.FieldCount;
                DbModelPropertyDef[] propertyDefs = new DbModelPropertyDef[len];
                MethodInfo[] setMethods = new MethodInfo[len];

                for (int i = 0; i < len; ++i)
                {
                    propertyDefs[i] = modelDef.GetDbPropertyDef(reader.GetName(i))!;
                    setMethods[i] = propertyDefs[i].SetMethod;
                }

                Func<IDbModelDefFactory, IDataReader, object> fullStack_mapper = DbModelConvert.CreateDataReaderRowToModelDelegate(
                    modelDef, reader, 0, modelDef.FieldCount, false);

                Func<IDataReader, object> dapper_mapper = DataReaderTypeMapper.GetTypeDeserializerImpl(typeof(T), reader);

                Func<IDataReader, object> reflection_mapper = (r) =>
                {
                    T item = Activator.CreateInstance<T>();

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
                    list1.Add((T)obj1);
                    stopwatch1.Stop();

                    stopwatch2.Start();
                    object obj2 = dapper_mapper(reader);
                    list2.Add((T)obj2);
                    stopwatch2.Stop();

                    stopwatch3.Start();
                    object obj3 = reflection_mapper(reader);
                    list3.Add((T)obj3);
                    stopwatch3.Stop();
                }

                time1 += stopwatch1.Elapsed;
                time2 += stopwatch2.Elapsed;
                time3 += stopwatch3.Elapsed;
            }

            Console.WriteLine("FullStack_Emit : " + (time1.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("Dapper : " + (time2.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine("FullStack_Reflection : " + (time3.TotalMilliseconds / (loop * 1.0)).ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public async Task Test_Mapper_ToModel_Performance()
        {
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Guid_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_BookModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Guid_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_PublisherModel>();
            await Test_Mapper_ToModel_Performance_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private void Test_Mapper_ToParameter_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            T model = Mocker.MockOne<T>();

            var emit_results = model.ToDbParameters(modelDef, Db.ModelDefFactory, null, 0);

            var reflect_results = model.ToDbParametersUsingReflection(modelDef, null, 0);

            AssertEqual(emit_results, reflect_results, modelDef.EngineType);
        }

        [TestMethod]
        public void Test_Mapper_ToParameter()
        {
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_BookModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_AutoIncrementId_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Guid_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_BookModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_AutoIncrementId_BookModel>();

            Test_Mapper_ToParameter_Core<MySql_Timestamp_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timestamp_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<MySql_Timeless_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Guid_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_PublisherModel>();
            Test_Mapper_ToParameter_Core<Sqlite_Timeless_Long_AutoIncrementId_PublisherModel>();
        }

        private void Test_Mapper_ToParameter_Performance_Core<T>() where T : IDbModel
        {
            var modelDef = Db.ModelDefFactory.GetDef<T>()!;
            var models = Mocker.Mock<T>(1000000);

            Stopwatch stopwatch = new Stopwatch();

            int i = 0;
            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParameters(modelDef, Db.ModelDefFactory, null, i++);
            }

            stopwatch.Stop();

            Console.WriteLine($"Emit: {stopwatch.ElapsedMilliseconds}");

            i = 0;
            stopwatch.Restart();
            foreach (var model in models)
            {
                _ = model.ToDbParametersUsingReflection(modelDef, null, i++);
            }
            stopwatch.Stop();

            Console.WriteLine($"Reflection: {stopwatch.ElapsedMilliseconds}");
        }

        [TestMethod]
        public void Test_Mapper_ToParameter_Performance()
        {
            Test_Mapper_ToParameter_Performance_Core<MySql_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Performance_Core<MySql_Timestamp_Guid_PublisherModel>();

            Test_Mapper_ToParameter_Performance_Core<Sqlite_Timestamp_Guid_BookModel>();
            Test_Mapper_ToParameter_Performance_Core<Sqlite_Timestamp_Guid_PublisherModel>();
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

        #endregion
    }
}