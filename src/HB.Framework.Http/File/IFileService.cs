using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HB.Framework.Http.File
{
    public interface IFileService
    {
        Task SetAvatarAsync(string userGuid, IFormFile file);
    }
}