/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Net.Http;
using HB.FullStack.Client.Components;
using HB.FullStack.Client.ApiClient;
using HB.FullStack.Common.Models;
using HB.FullStack.Client.Abstractions;

namespace HB.FullStack.Client.Base
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : ApiResource
    {
        public TokenRefreshHttpClientHandler AutoRefreshedHttpClientHandler { get; }

        protected BaseFileRepo(
            IApiClient apiClient,
            ITokenPreferences clientPreferences,
            TokenRefreshHttpClientHandler tokenAutoRefreshedHttpClientHandler)
            : base(apiClient, clientPreferences)
        {
            AutoRefreshedHttpClientHandler = tokenAutoRefreshedHttpClientHandler;
        }

        protected HttpClient CreateHttpClient()
        {
            return new HttpClient(AutoRefreshedHttpClientHandler);
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