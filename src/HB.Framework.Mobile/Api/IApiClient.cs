using HB.Framework.Common.Api;
using System.Threading.Tasks;

namespace HB.Framework.Client.Api
{
    public interface IApiClient
    {
        Task<T?> SendAsync<T>(ApiRequest request) where T : class;
        Task SendAsync(ApiRequest request);
        Task<bool> RefreshJwtAsync(EndpointSettings endpointSettings);
    }
}