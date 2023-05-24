using System;
using System.Collections.Generic;

using HB.FullStack.Common.IdGen;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.Utility
{
    [TestClass()]
    public class SecurityUtilTests
    {
        [TestMethod()]
        [DataRow(100000000)]
        public void CreateSequentialGuidTest(int numbers)
        {
            HashSet<Guid> ids = new HashSet<Guid>();

            for (int i = 0; i < numbers; i++)
            {
                Guid guid = StaticIdGen.GetSequentialGuid(GuidStoredFormat.AsBinary);

                if (!ids.Add(guid))
                {
                    Assert.Fail();
                }
            }

            Assert.IsTrue(numbers == ids.Count);
        }
    }
}