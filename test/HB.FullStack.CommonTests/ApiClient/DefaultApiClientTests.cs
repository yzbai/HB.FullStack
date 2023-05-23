global using static HB.FullStack.BaseTest.ApiConstants;

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using HB.FullStack.BaseTest;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.Test;
using HB.FullStack.Database.Engine;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.ApiClient
{
    [ResEndpoint(SiteName = ApiEndpointName)]
    public class BookRes : SharedResource
    {
        public override Guid? Id { get; set; }

        public override long? ExpiredAt { get; set; }

        public string? Name { get; set; }

        public string? Title { get; set; }

        public double Price { get; set; }
    }

    [TestClass()]
    public class DefaultApiClientTests : BaseTestClass
    {
        public DefaultApiClientTests() : base(DbEngineType.SQLite)
        {
        }

        [TestMethod()]
        public async Task GetAsyncTest()
        {
            PreferenceProvider.OnTokenFetched(
                new Common.Shared.TokenRes
                {
                    UserId = Guid.NewGuid(),
                    ExpiredAt = TimeUtil.UtcNow.AddHours(1).Ticks,
                    Mobile = null,
                    Email = null,
                    LoginName = null,
                    AccessToken = Guid.NewGuid().ToString(),
                    RefreshToken = Guid.NewGuid().ToString()
                });

            TestHttpServer httpServer = StartHttpServer(
                new TestRequestHandler($"", HttpMethod.Get, (request, response, parameters) =>
                {
                }),
                new TestRequestHandler($"api/{ApiVersion}/Book/ByName", HttpMethod.Get, (request, response, parameters) =>
                {
                    using StreamReader streamReader = new StreamReader(request.InputStream);
                    string requestJson = streamReader.ReadToEnd();

                    GetBookByNameRequest? getBookByNameRequest = SerializeUtil.FromJson<GetBookByNameRequest>(requestJson);

                    //Assert.IsNull(getBookByNameRequest.RequestBuilder);

                    string name = parameters!.FirstOrDefault(p => p.Key == "Name")!.Value;

                    BookRes res = new BookRes { Title = "T", Name = name, Price = 12.123 };

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
        public GetBookByNameRequest(string name) : base(null, "ByName")
        {
            Name = name;
        }

        [RequestQuery]
        public string? Name { get; set; }
    }
}