using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.Client.Api;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Resources;

namespace HB.FullStack.Client.Repos
{
    public abstract class BaseFileRepo<TRes> : BaseRepo where TRes : Resource
    {
        protected IApiClient ApiClient { get; }

        public BaseFileRepo(IApiClient apiClient)
        {
            ApiClient = apiClient;
        }

        public Task UploadAsync(string fileSuffix, IEnumerable<byte[]> fileDatas, IEnumerable<TRes> resources)
        {
            InsureInternet();

            string suffix = fileSuffix.StartsWith('.') ? fileSuffix : "." + fileSuffix;

            var fileNames = resources.Select(r => $"{r.Id}{fileSuffix}");

            FileUpdateRequest<TRes> request = new FileUpdateRequest<TRes>(fileDatas, fileNames);

            request.Resources.AddRange(resources);

            return ApiClient.UpdateAsync(request);
        }
    }
}