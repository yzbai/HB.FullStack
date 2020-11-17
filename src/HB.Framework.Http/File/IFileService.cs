using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HB.Framework.Server.File
{
    public interface IFileService
    {
        Task SetAvatarAsync(string userGuid, IFormFile file);
    }
}