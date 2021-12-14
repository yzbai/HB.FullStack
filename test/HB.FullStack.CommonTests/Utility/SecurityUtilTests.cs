using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Tests
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
                Guid guid = SecurityUtil.CreateSequentialGuid(DateTimeOffset.Now, GuidStoredFormat.AsBinary);

                if(!ids.Add(guid))
                {
                    Assert.Fail();
                }
            }

            Assert.IsTrue(numbers == ids.Count);
        }
    }
}