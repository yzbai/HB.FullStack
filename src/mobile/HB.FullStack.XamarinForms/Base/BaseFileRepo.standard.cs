using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.Common.Api;
using Xamarin.Forms;
using System.Net.Http;
using HB.FullStack.Common;
using System;
using HB.FullStack.Common.ApiClient;

namespace HB.FullStack.XamarinForms.Base
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : ApiResource2
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

        protected IApiClient ApiClient { get; }

        protected BaseFileRepo(IApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        protected HttpClient CreateHttpClient()
        {
            return new HttpClient(HttpClientHandler);
        }

        /// <exception cref="System.ApiException"></exception>
        //public Task UploadAsync(string fileSuffix, byte[] file)
        //{
        //    EnsureInternet();

        //    string suffix = fileSuffix.StartsWith(".", System.StringComparison.InvariantCulture) ? fileSuffix : "." + fileSuffix;

        //    var fileNames = resources switch
        //    {
        //        IEnumerable<LongIdResource> longLst => longLst.Select(r => $"{r.Id}{fileSuffix}").ToList(),
        //        IEnumerable<GuidResource> guidLst => guidLst.Select(r => $"{r.Id}{fileSuffix}").ToList(),
        //        _ => throw MobileExceptions.UploadError($"目前不能处理GuidModelObject或者LongIdModelObject之外的类。当前类型为 {typeof(TRes).FullName}")
        //    };

        //    UploadRequest<TRes> request = new UploadRequest<TRes>(fileDatas, fileNames, resources);

        //    return ApiClient.UploadAsync(request);
        //}
    }
}