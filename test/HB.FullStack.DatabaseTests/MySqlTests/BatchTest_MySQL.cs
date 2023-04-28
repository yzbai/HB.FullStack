using System;
using System.Threading.Tasks;
using HB.FullStack.BaseTest.Data.MySqls;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.MySQL
{
    [TestClass]
    public class BatchTest_MySQL : BaseTestClass
    {

        [TestMethod]
        public async Task Batch_Add_AutoIdTimeless_Test()
        {
            //Timeless
            var timelessLst = Mocker.GetAutoIdBTTimelesses(10);

            await Db.AddAsync(timelessLst, "Tester", null);

            long count = await Db.CountAsync<AutoIdBTTimeless>(null);

            Assert.AreEqual(count, timelessLst.Count);
        }

        [TestMethod]
        public async Task Batch_Add_AutoIdTimestamp_Test()
        {
            //Timeless
            var lst = Mocker.GetAutoIdBTTimestamps(10);

            await Db.AddAsync(lst, "Tester", null);

            long count = await Db.CountAsync<AutoIdBTTimestamp>(null);

            Assert.AreEqual(count, lst.Count);
        }

        [TestMethod]
        public async Task Batch_Update_AutoIdTimestamp_Test()
        {
            //Timeless
            var lst = Mocker.GetAutoIdBTTimestamps(10);

            await Db.AddAsync(lst, "Tester", null);

            foreach (var t in lst)
            {
                t.Name = "Modified";
                t.Age = 100;
            }

            await Db.UpdateAsync(lst, "dd", null);

            var rt = await Db.ScalarAsync<AutoIdBTTimestamp>(lst[0].Id, null);

            Assert.IsTrue(SerializeUtil.ToJson(rt) == SerializeUtil.ToJson(lst[0]));
        }

    }
}
