using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class SqlTests : DatabaseTestClass
    {
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
    }
}
