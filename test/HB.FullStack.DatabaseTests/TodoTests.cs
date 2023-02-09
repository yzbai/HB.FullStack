using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.BaseTest.Data.MySqls;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.DatabaseTests
{
    [TestClass]
    public class TodoTests : BaseTestClass
    {
        [TestMethod]
        public async Task Test_FieldLength_OversizeAsync()
        {
            //TODO: 测试指定字段长度为10，结果赋值字符串长度为100，怎么处理

            FieldLengthTestModel model = new FieldLengthTestModel { Content = "12345678910" };

            var ex = await Assert.ThrowsExceptionAsync<DbException>(() => Db.AddAsync(model, "", null));

            Assert.IsTrue(ex.ErrorCode == ErrorCodes.DbDataTooLong);
        }
    }
}
