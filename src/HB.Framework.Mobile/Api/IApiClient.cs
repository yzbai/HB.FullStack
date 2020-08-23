using HB.Framework.Common.Api;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public interface IApiClient
    {
        Task<ApiResponse<T>> RequestAsync<T>(ApiRequest request) where T : ApiResponseData;
        Task<ApiResponse> RequestAsync(ApiRequest request);
        Task<ApiResponse<T>> RefreshJwtAsync<T>(JwtApiRequest? request, ApiResponse response, EndpointSettings endpointSettings) where T : ApiResponseData;
    }
}