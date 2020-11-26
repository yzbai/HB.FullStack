using HB.FullStack.Common.Api;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Api
{
    public interface IApiClient
    {
        Task<T> SendAsync<T>(ApiRequest request) where T : class;
        Task SendAsync(ApiRequest request);
        Task<bool> RefreshJwtAsync(EndpointSettings endpointSettings);
    }
}