using HB.FullStack.CommonTests.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.Common.Api.Tests
{
    [TestClass()]
    public class EndpointBindingFactoryTests
    {
        [TestMethod()]
        public void GetTest()
        {
            EndpointBinding def = EndpointBindingFactory.Get<BookRes>();

            Assert.IsNotNull(def);

            Assert.IsNotNull(def.EndpointName);
            Assert.IsNotNull(def.ControllerModelName);
            Assert.IsNotNull(def.Version);


        }
    }
}