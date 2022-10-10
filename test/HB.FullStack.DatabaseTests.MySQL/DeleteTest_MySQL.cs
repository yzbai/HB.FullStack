using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;
using HB.FullStack.DatabaseTests.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.MySQL
{
    public class DeleteTimestampModel : TimestampGuidDbModel
    {
        public string? Name { get; set; }
    }

    public class DeleteTimelessModel : TimelessGuidDbModel
    {
        public string? Name { get; set; }
    }

    [TestClass]
    public class DeleteTest_MySQL : BaseTestClass
    {
        private class Mocker
        {
            public static DeleteTimelessModel MockTimelessModel()
            {
                return new DeleteTimelessModel { Name = "xx" };
            }

            public static DeleteTimestampModel MockTimestampModel()
            {
                return new DeleteTimestampModel { Name = "TTT" };
            }

            public static IEnumerable<DeleteTimelessModel> MockTimelessList(int count = 10)
            {
                var lst = new List<DeleteTimelessModel>();

                for (int i = 0; i < count; i++)
                {
                    lst.Add(new DeleteTimelessModel { Name = SecurityUtil.CreateRandomString(5) });
                }

                return lst;
            }
            public static IEnumerable<DeleteTimestampModel> MockTimestampList(int count = 10)
            {
                var lst = new List<DeleteTimestampModel>();

                for (int i = 0; i < count; i++)
                {
                    lst.Add(new DeleteTimestampModel { Name = SecurityUtil.CreateRandomString(5) });
                }

                return lst;
            }
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Delete_TimelessAsync(bool trulyDelete)
        {
            var model = Mocker.MockTimelessModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<DeleteTimelessModel>(model.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));

            await Db.DeleteAsync<DeleteTimelessModel>(model.Id, null, "", trulyDelete);

            var rt2 = await Db.ScalarAsync<DeleteTimestampModel>(model.Id, null);

            Assert.IsNull(rt2);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Delete_TimestampAsync(bool trulyDelete)
        {
            var model = Mocker.MockTimestampModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<DeleteTimestampModel>(model.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));

            await Db.DeleteAsync<DeleteTimestampModel>(model.Id, model.Timestamp, "", null, trulyDelete);

            var rt2 = await Db.ScalarAsync<DeleteTimestampModel>(model.Id, null);

            Assert.IsNull(rt2);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Delete_ModelAsync(bool trulyDelete)
        {
            var model = Mocker.MockTimestampModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<DeleteTimestampModel>(model.Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(model) == SerializeUtil.ToJson(rt));

            await Db.DeleteAsync<DeleteTimestampModel>(model, "", null, trulyDelete);

            var rt2 = await Db.ScalarAsync<DeleteTimestampModel>(model.Id, null);

            Assert.IsNull(rt2);
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Batch_Delete_Timeless(bool trulyDelete)
        {
            var models = Mocker.MockTimelessList(3);

            await Db.BatchAddAsync(models, "", null);

            var ids = models.Select<DeleteTimelessModel, object>(m => m.Id).ToList();

            await Db.BatchDeleteAsync<DeleteTimelessModel>(ids, null, "", trulyDelete);

            var rts = await Db.RetrieveAsync<DeleteTimelessModel>(m => SqlStatement.In(m.Id, false, ids), null);

            Assert.IsTrue(rts.IsNullOrEmpty());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Batch_Delete_Timestamp(bool trulyDelete)
        {
            var models = Mocker.MockTimestampList(3);

            await Db.BatchAddAsync(models, "", null);

            var ids = models.Select<DeleteTimestampModel, object>(m => m.Id).ToList();

            List<long?> timestamps = models.Select<DeleteTimestampModel, long?>(m => m.Timestamp).ToList();

            await Db.BatchDeleteAsync<DeleteTimestampModel>(ids, timestamps, "", null, trulyDelete);

            var rts = await Db.RetrieveAsync<DeleteTimestampModel>(m => SqlStatement.In(m.Id, false, ids), null);

            Assert.IsTrue(rts.IsNullOrEmpty());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Batch_Delete_TimestampModel(bool trulyDelete)
        {
            var models = Mocker.MockTimestampList(3);

            await Db.BatchAddAsync(models, "", null);

            var ids = models.Select<DeleteTimestampModel, object>(m => m.Id).ToList();

            await Db.BatchDeleteAsync<DeleteTimestampModel>(models, "", null, trulyDelete);

            var rts = await Db.RetrieveAsync<DeleteTimestampModel>(m => SqlStatement.In(m.Id, false, ids), null);

            Assert.IsTrue(rts.IsNullOrEmpty());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task Test_Batch_Delete_TimelessModel(bool trulyDelete)
        {
            var models = Mocker.MockTimelessList(3);

            await Db.BatchAddAsync(models, "", null);

            var ids = models.Select<DeleteTimelessModel, object>(m => m.Id).ToList();

            await Db.BatchDeleteAsync<DeleteTimelessModel>(models, "", null, trulyDelete);

            var rts = await Db.RetrieveAsync<DeleteTimelessModel>(m => SqlStatement.In(m.Id, false, ids), null);

            Assert.IsTrue(rts.IsNullOrEmpty());
        }
    }
}
