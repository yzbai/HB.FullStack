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
            ApiResourceBinding def = ApiResourceBindingFactory.Get<BookRes>();

            Assert.IsNotNull(def);

            Assert.IsNotNull(def.EndpointName);
            Assert.IsNotNull(def.ControllerModelName);
            Assert.IsNotNull(def.Version);


        }
    }
}