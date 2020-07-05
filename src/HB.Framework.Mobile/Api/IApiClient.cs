using HB.Framework.Common.Api;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public interface IApiClient
    {
        Task<ApiResponse<T>> GetAsync<T>(ApiRequest request) where T : ApiResponseData;
        Task<ApiResponse> GetAsync(ApiRequest request);
    }
}