using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.Common.Api;
using Xamarin.Forms;
using System.Net.Http;
using HB.FullStack.Common;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : ApiResource
    {
        private static TokenAutoRefreshedHttpClientHandler? _httpClientHandler;
        private static TokenAutoRefreshedHttpClientHandler HttpClientHandler
        {
            get
            {
                if (_httpClientHandler == null)
                {
                    _httpClientHandler = DependencyService.Resolve<TokenAutoRefreshedHttpClientHandler>();
                }

                return _httpClientHandler;
            }
        }

        protected static MemorySimpleLocker _requestLocker = new MemorySimpleLocker();

        protected IApiClient ApiClient { get; }

        protected BaseFileRepo(IApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        protected HttpClient GetHttpClient()
        {
            return new HttpClient(HttpClientHandler);
        }

        /// <exception cref="System.ApiException"></exception>
        public Task UploadAsync(string fileSuffix, IEnumerable<byte[]> fileDatas, IEnumerable<TRes> resources)
        {
            EnsureInternet();

            string suffix = fileSuffix.StartsWith(".", System.StringComparison.InvariantCulture) ? fileSuffix : "." + fileSuffix;

            var fileNames = resources.Select(r => $"{r.Id}{fileSuffix}").ToList();

            FileUpdateRequest<TRes> request = new FileUpdateRequest<TRes>(fileDatas, fileNames, resources);

            return ApiClient.UpdateAsync(request);
        }
    }
}