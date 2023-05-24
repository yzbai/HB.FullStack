using System;
using System.Web;

using HB.FullStack.Common.Convert;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.Convert
{
    [TestClass()]
    public class ConvertCenterTests
    {
        [TestMethod()]
        public void ConvertToStringTest()
        {
            string str = "This is a Text(xx);$#$";
            var result = StringConvertCenter.ConvertToString(str, null, StringConvertPurpose.NONE);

            Assert.AreEqual(str, result);

            var result2 = StringConvertCenter.ConvertToString(str, null, StringConvertPurpose.HTTP_QUERY);

            Assert.AreEqual(HttpUtility.UrlEncode(str), result2);

            //TODO: Continue
        }
    }
}