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

namespace HB.FullStack.DatabaseTests.MySqlTests
{
    public enum ConvertTestEnum
    {
        None,
        This,
        Is,
        Fun
    }
    public class ConvertTestModel : TimelessGuidDbModel
    {
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }

        public ConvertTestEnum TestEnum { get; set; }

        public static ConvertTestModel GetRandomModel()
        {
            return new ConvertTestModel { DateOnly = new DateOnly(2022, 10, 22), TimeOnly = new TimeOnly(10, 23, 23, 877, 342), TestEnum = ConvertTestEnum.This };
        }
    }

    [TestClass]
    public class ConvertTest : BaseTestClass
    {
        [TestMethod]
        public async Task DateOnly_Test()
        {
            //TODO: finish this test
            ConvertTestModel model = ConvertTestModel.GetRandomModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<ConvertTestModel>(model.Id, null);

            Assert.AreEqual(model.DateOnly, rt!.DateOnly);
            Assert.AreEqual(model.TimeOnly, rt!.TimeOnly);
        }

        [TestMethod]
        public async Task Enum_Test()
        {
            ConvertTestModel model = ConvertTestModel.GetRandomModel();

            await Db.AddAsync(model, "", null);

            var rt = await Db.ScalarAsync<ConvertTestModel>(t=>t.TestEnum == ConvertTestEnum.This, null);

            Assert.AreEqual(rt?.Id, model.Id);
        }
    }
}