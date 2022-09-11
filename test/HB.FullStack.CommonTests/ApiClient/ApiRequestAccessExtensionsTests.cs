using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.Common.ApiClient.Tests
{
    public class TestGetRequest : ApiRequest
    {
        public TestGetRequest(string resName, ApiMethod apiMethod, ApiRequestAuth2 auth, string condition) : base(resName, apiMethod, auth, condition)
        {
        }

        public string Name { get; set; } = "TestName";

        public int Age { get; set; } = 123;

        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

        public string[] Times { get; set; } = new string[] { "a", null };
        //public IEnumerable Times2 { get; set; } = new string[] { "a", null };

        public IEnumerable<string> Times3 { get; set; } = new string[] { "a", "xxx", "ssd" };

        public IEnumerable<string> Times4 { get; set; } = new List<string> { "sfasf", "sfasf" };

        public InnerCls Inner { get; set; } = new InnerCls();

    }

    public class InnerCls
    {
        public string Name { get; set; } = "TestName";

        public int Age { get; set; } = 123;

        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTimeOffset Now { get; set; } = DateTimeOffset.UtcNow;

        public string[] Times { get; set; } = new string[] { "a", null };
        //public IEnumerable Times2 { get; set; } = new string[] { "a", null };

        public IEnumerable<string> Times3 { get; set; } = new string[] { "a", "xxx", "ssd" };

        public IEnumerable<string> Times4 { get; set; } = new List<string> { "sfasf", "sfasf" };

    }
    [TestClass()]
    public class ApiRequestAccessExtensionsTests
    {
        [TestMethod()]
        public void BuildQueryStringTest()
        {

        }
    }
}