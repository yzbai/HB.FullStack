using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Meta;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.Meta
{
    [TestClass]
    public class AttributeTests
    {
        [TestMethod]
        public void GetCustomAttribute_InheritTest()
        {
            var r0 = typeof(ChildTestObj).GetCustomAttribute<RootAttribute>();
            var r1 = typeof(ChildTestObj).GetCustomAttribute<RootAttribute>(false);
            var r2 = typeof(ChildTestObj).GetCustomAttribute<RootAttribute>(true);

            Assert.IsTrue(r0 != null);
            Assert.IsTrue(r1 == null);
            Assert.IsTrue(r2 != null);
        }

        public class RootAttribute : Attribute { }

        public class ChildAttribute : Attribute { }

        [Root]
        class RootTestObj { }

        [Child]
        class ChildTestObj : RootTestObj { }
    }
}
