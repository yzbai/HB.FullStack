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
    public class ConvertTestModel : TimelessGuidDbModel
    {
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
    }

    [TestClass]
    public class ConvertTest : BaseTestClass
    {
        [TestMethod]
        public async Task DateOnly_Test()
        {
            //TODO: finish this test
            ConvertTestModel model = GetRandomModel();

            await Db.AddAsync(model, "", null);
        }

        private ConvertTestModel GetRandomModel()
        {
            return new ConvertTestModel { DateOnly = new DateOnly(2022, 10, 22), TimeOnly = new TimeOnly(10, 23) };
        }

        [TestMethod]
        public void TimeOnly_Test()
        {
        }
    }
}