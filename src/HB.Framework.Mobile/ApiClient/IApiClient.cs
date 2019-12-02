using HB.Framework.Common.Mobile;
using System.Threading.Tasks;

namespace HB.Framework.Mobile.ApiClient
{
    public interface IApiClient
    {
        Task<ApiResponse<T>> GetAsync<T>(ApiRequest request) where T : ApiData;
    }
}