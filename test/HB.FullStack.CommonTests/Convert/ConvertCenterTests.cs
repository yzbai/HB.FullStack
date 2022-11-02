using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.Common.Convert;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Convert.Tests
{
    [TestClass()]
    public class ConvertCenterTests
    {
        [TestMethod()]
        public void ConvertToStringTest()
        {
            int? value = 10;
            var result = StringConvertCenter.ConvertToString(value, null, StringConvertPurpose.NONE);
            
            //TODO: Continue
        }
    }
}