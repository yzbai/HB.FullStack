using System;
using System.Net.Http;

using HB.FullStack.Client.Network;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.ApiClient;

namespace HB.FullStack.Client
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : ApiResource
    {
        public UserTokenRefreshHttpClientHandler TokenAutoRefreshedHttpClientHandler { get; }

        protected BaseFileRepo(
            IApiClient apiClient,
            IPreferenceProvider userPreferenceProvider,
            UserTokenRefreshHttpClientHandler tokenAutoRefreshedHttpClientHandler,
            StatusManager connectivityManager) : base(apiClient, userPreferenceProvider, connectivityManager)
        {
            TokenAutoRefreshedHttpClientHandler = tokenAutoRefreshedHttpClientHandler;
        }

        protected HttpClient CreateHttpClient()
        {
            return new HttpClient(TokenAutoRefreshedHttpClientHandler);
        }

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