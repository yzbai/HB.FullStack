using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.FullStack.Common.Api;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.CommonTests.Data;

namespace HB.FullStack.Common.Api.Tests
{
    [TestClass()]
    public class ApiResourceDefFactoryTests
    {
        [TestMethod()]
        public void GetTest()
        {
            ApiResourceDef def = ApiResourceDefFactory.Get<BookRes>();

            Assert.IsNotNull(def);

            Assert.IsNotNull(def.EndpointName);
            Assert.IsNotNull(def.ResName);
            Assert.IsNotNull(def.ApiVersion);


        }
    }
}