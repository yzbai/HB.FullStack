using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace HB.Infrastructure.Tencent
{
    internal class TCapthaClient : ITCapthaClient
    {
        private readonly ILogger logger;
        private readonly TCapthaOptions options;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDictionary<string, ApiKeySetting> apiKeySettings;

        public TCapthaClient(ILogger<TCapthaClient> logger, IOptions<TCapthaOptions> options, IHttpClientFactory httpClientFactory)
        {
            this.logger = logger;
            this.options = options.Value;
            this.httpClientFactory = httpClientFactory;

            apiKeySettings = this.options.ApiKeySettings.ToDictionary(s => s.AppId);
        }

        public async Task<bool> VerifyTicket(string appid, string ticket, string randstr, string userIp)
        {
            ThrowIf.NullOrEmpty(ticket, nameof(ticket));

            ThrowIf.NullOrEmpty(randstr, nameof(randstr));

            ThrowIf.NullOrEmpty(userIp, nameof(userIp));

            if (!apiKeySettings.TryGetValue(appid.ThrowIfNullOrEmpty(nameof(appid)), out ApiKeySetting apiKeySetting))
            {
                logger.LogError($"lack ApiKeySettings for AppId:{appid}");
            }

            string query = new Dictionary<string, string> {
                { "aid", apiKeySetting.AppId },
                { "AppSecretKey", apiKeySetting.AppSecretKey},
                {"Ticket",  ticket},
                {"Randstr", randstr },
                {"UserIP", userIp }
            }.ToHttpValueCollection().ToString();

            string requestUrl = options.Endpoint.RemoveSuffix("/") + "?" + query;

            using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUrl))
            {
                HttpClient httpClient = httpClientFactory.CreateClient(TCapthaOptions.EndpointName);

                string content;

                using (HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage).ConfigureAwait(false))
                {
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        return false;
                    }

                    content = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                try
                {
                    int result = Convert.ToInt32(JsonUtil.FromJson(content, "response"), GlobalSettings.Culture);

                    if (result == 1)
                    {
                        return true;
                    }

                    return false;
                }
                catch(Exception ex)
                {
                    logger.LogError(ex, $"TCaptha Response Parse Error. Content:{content}");

                    return false;
                }
            }
        }


    }
}

