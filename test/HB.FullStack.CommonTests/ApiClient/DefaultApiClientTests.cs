using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Threading.Tasks;
using HB.FullStack.CommonTests.Data;
using HB.FullStack.Common.Api.Requests;
using HB.FullStack.Common.Api;
using System.IO;
using System.Text.Json.Serialization;
using HB.FullStack.Common.Test;

namespace HB.FullStack.CommonTests.ApiClient
{
    [TestClass()]
    public class DefaultApiClientTests : ApiTestBaseClass
    {
        [TestMethod()]
        public void GetStreamAsyncTest()
        {

        }


        [TestMethod()]
        public async Task GetAsyncTest()
        {
            PreferenceProvider.OnTokenFetched(userId: Guid.NewGuid(), userCreateTime: DateTimeOffset.Now, mobile: null, email: null, loginName: null, accessToken: Guid.NewGuid().ToString(), refreshToken: Guid.NewGuid().ToString());

            TestHttpServer httpServer = StartHttpServer(
                new TestRequestHandler($"/api/{ApiVersion}/BookRes/ByName", HttpMethodName.Get, (request, response, parameters) =>
                {
                    using StreamReader streamReader = new StreamReader(request.InputStream);
                    string requestJson = streamReader.ReadToEnd();

                    GetBookByNameRequest getBookByNameRequest = SerializeUtil.FromJson<GetBookByNameRequest>(requestJson);

                    Assert.IsNull(getBookByNameRequest.RequestBuilder);

                    BookRes res = new BookRes { Title = "T", Name = getBookByNameRequest.Name, Price = 12.123 };

                    response.ContentType = "application/json";

                    string json = SerializeUtil.ToJson(res);

                    using StreamWriter streamWriter = new StreamWriter(response.OutputStream);
                    streamWriter.Write(json);
                }));

            BookRes bookRes = await ApiClient.GetAsync<BookRes>(new GetBookByNameRequest("TestBook"));

            Assert.IsNotNull(bookRes);
        }


        [TestMethod()]
        public void SendAsyncTest()
        {
            Assert.Fail();
        }
    }

    public class GetBookByNameRequest : GetRequest<BookRes>
    {
        public string Name { get; set; }

        [JsonConstructor]
        public GetBookByNameRequest() { }

        public GetBookByNameRequest(string name) : base("ByName")
        {
            Name = name;
        }
    }
}