global using static HB.FullStack.BaseTest.ApiConstants;

using System;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using HB.FullStack.BaseTest;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Test;
using HB.FullStack.CommonTests.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.ApiClient
{
    [TestClass()]
    public class DefaultApiClientTests : BaseTestClass
    {
        [TestMethod()]
        public void GetStreamAsyncTest()
        {

        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            PreferenceProvider.OnTokenFetched(
                new Common.Shared.TokenRes {
                UserId = Guid.NewGuid(),
                TokenCreatedTime =  DateTimeOffset.Now,
                Mobile = null,
                Email = null,
                LoginName = null,
                AccessToken = Guid.NewGuid().ToString(),
                RefreshToken = Guid.NewGuid().ToString()});

            TestHttpServer httpServer = StartHttpServer(
                new TestRequestHandler($"/api/{ApiVersion}/BookRes/ByName", HttpMethod.Get, (request, response, parameters) =>
                {
                    using StreamReader streamReader = new StreamReader(request.InputStream);
                    string requestJson = streamReader.ReadToEnd();

                    GetBookByNameRequest? getBookByNameRequest = SerializeUtil.FromJson<GetBookByNameRequest>(requestJson);

                    //Assert.IsNull(getBookByNameRequest.RequestBuilder);

                    BookRes res = new BookRes { Title = "T", Name = getBookByNameRequest!.Name!, Price = 12.123 };

                    response.ContentType = "application/json";

                    string json = SerializeUtil.ToJson(res);

                    using StreamWriter streamWriter = new StreamWriter(response.OutputStream);
                    streamWriter.Write(json);
                }));

            BookRes? bookRes = await ApiClient.GetAsync<BookRes>(new GetBookByNameRequest("TestBook"));

            Assert.IsNotNull(bookRes);
        }

        [TestMethod()]
        public void SendAsyncTest()
        {
            //TODO: Continue
        }
    }

    public class GetBookByNameRequest : GetRequest<BookRes>
    {
        public string? Name { get; set; }

        [JsonConstructor]
        public GetBookByNameRequest() { }

        public GetBookByNameRequest(string name)
        {
            Name = name;
        }
    }
}