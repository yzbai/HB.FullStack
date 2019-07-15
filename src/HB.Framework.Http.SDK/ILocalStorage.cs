using System.Threading.Tasks;

namespace HB.Framework.Http.SDK
{
    public interface ILocalStorage
    {
        Task<string> GetAsync(string key);

        Task SetAsync(string key, string value);

    }
}
