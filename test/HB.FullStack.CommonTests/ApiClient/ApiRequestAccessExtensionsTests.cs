using System;
using System.Collections.Generic;

using HB.FullStack.Common.Api;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.Common.ApiClient.Tests
{
    public class TestRequestObject
    {
        [RequestQuery]
        public string Name { get; set; } = "TestName";

        [RequestQuery]
        public int Age { get; set; } = 123;

        [RequestQuery]
        public Guid Id { get; set; } = new Guid("4DF758FB-F612-40B2-A5B1-DAF48E7F8EF7");

        [RequestQuery]
        public DateTimeOffset Now { get; set; } = new DateTimeOffset(2022, 12, 22, 22, 22, 22, TimeSpan.Zero);

        [RequestQuery]
        public string[] Times { get; set; } = new string[] { "a", null };
        //public IEnumerable Times2 { get; set; } = new string[] { "a", null };

        [RequestQuery]
        public IEnumerable<string> Times3 { get; set; } = new string[] { "a", "xxx", "ssd" };

        [RequestQuery]
        public IEnumerable<string> Times4 { get; set; } = new List<string> { "sfasf", "sfasf" };

        [RequestQuery]
        public InnerTestRequestObject Inner { get; set; } = new InnerTestRequestObject();

        [RequestBody]
        public InnerTestRequestObject InnerBody { get; set; } = new InnerTestRequestObject();

    }

    public class InnerTestRequestObject
    {
        public string Name { get; set; } = "TestName";

        public int Age { get; set; } = 123;

        public Guid Id { get; set; } = new Guid("4DF758FB-F612-40B2-A5B1-DAF48E7F8EF7");

        public DateTimeOffset Now { get; set; } = new DateTimeOffset(2022, 12, 22, 22, 22, 22, TimeSpan.Zero);

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

        [TestMethod()]
        public void BuildQueryStringTest1()
        {
            TestRequestObject to = new TestRequestObject();

            string result = "Name=TestName&Age=123&Id=4df758fb-f612-40b2-a5b1-daf48e7f8ef7&Now=2022%2f12%2f22+22%3a22%3a22+%2b00%3a00&Times=a&Times=&Times3=a&Times3=xxx&Times3=ssd&Times4=sfasf&Times4=sfasf&Inner.Name=TestName&Inner.Age=123&Inner.Id=4df758fb-f612-40b2-a5b1-daf48e7f8ef7&Inner.Now=2022%2f12%2f22+22%3a22%3a22+%2b00%3a00&Inner.Times=a&Inner.Times=&Inner.Times3=a&Inner.Times3=xxx&Inner.Times3=ssd&Inner.Times4=sfasf&Inner.Times4=sfasf";
            string queryString = to.BuildQueryString();
            Assert.AreEqual(result, queryString);
        }

        [TestMethod()]
        public void GetRequestBodyTest()
        {
            TestRequestObject to = new TestRequestObject();

            string? json = SerializeUtil.ToJson(to.GetRequestBody());

            string result = "{\"Name\":\"TestName\",\"Age\":123,\"Id\":\"4df758fb-f612-40b2-a5b1-daf48e7f8ef7\",\"Now\":\"2022-12-22T22:22:22+00:00\",\"Times\":[\"a\",null],\"Times3\":[\"a\",\"xxx\",\"ssd\"],\"Times4\":[\"sfasf\",\"sfasf\"]}";

            Assert.AreEqual(json, result);

        }
    }
}