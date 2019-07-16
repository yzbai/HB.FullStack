using System.Threading.Tasks;

namespace HB.Framework.Http.SDK
{
    public interface IResourceClient
    {
        Task<Resource<T>> GetAsync<T>(ResourceRequest request) where T : ResourceResponse;
    }
}