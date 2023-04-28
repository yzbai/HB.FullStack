using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.Common.Test.Tests
{
    [TestClass()]
    public class TestHttpServerTests
    {
        [TestMethod()]
        public async Task TestHttpServerTest()
        {
            string relativeUrl = "Test";

            using TestHttpServer server = new TestHttpServer(0, relativeUrl, HttpMethod.Get, (request, response, queryDict) =>
            {
                using StreamWriter writer = new StreamWriter(response.OutputStream);

                writer.WriteLine("<html><body>");
                writer.WriteLine("<h1>Test Ok.</h1>");
                writer.WriteLine("<ul>");
                foreach (KeyValuePair<string, string> kv in queryDict!)
                {
                    writer.WriteLine($"<li>{kv.Key} - {kv.Value}</li>");
                }
                writer.WriteLine("</ul>");
                writer.WriteLine("</body></html>");

            }, "localhost");

            string url = $"http://localhost:{server.Port}/{relativeUrl}?ParamA=ABC&ParamB=DEF";

            using HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = await httpClient.GetAsync(url);

            string content = await response.Content.ReadAsStringAsync();

            Assert.IsTrue(response.StatusCode == System.Net.HttpStatusCode.OK);
        }
    }
}