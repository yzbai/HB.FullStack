/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests.SQLite
{
    [DbTable(BaseTestClass.DbSchema_Sqlite)]
    public class ConvertTestModel_Sqlite : TimelessGuidDbModel
    {
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }

        public static ConvertTestModel_Sqlite GetRandomModel()
        {
            return new ConvertTestModel_Sqlite { DateOnly = new DateOnly(2022, 10, 22), TimeOnly = new TimeOnly(10, 23, 23, 877, 342) };
        }
    }

    [TestClass]
    public class ConvertTest : BaseTestClass
    {
        [TestMethod]
        public async Task DateOnly_Test()
        {
            //TODO: finish this test
            ConvertTestModel_Sqlite model = ConvertTestModel_Sqlite.GetRandomModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<ConvertTestModel_Sqlite>(model.Id, null);

            Assert.AreEqual(model.DateOnly, rt!.DateOnly);
            Assert.AreEqual(model.TimeOnly, rt!.TimeOnly);
        }

        [TestMethod]
        public void TimeOnly_Test()
        {
        }
    }
}