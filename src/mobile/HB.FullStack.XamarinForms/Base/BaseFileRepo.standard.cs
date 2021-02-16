using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.XamarinForms.Api;
using HB.FullStack.Common.Api;

namespace HB.FullStack.XamarinForms.Repos
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : ApiResource
    {
        protected IApiClient ApiClient { get; }

        protected BaseFileRepo(IApiClient apiClient)
        {
            ApiClient = apiClient;
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