using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HB.FullStack.Server.File
{
    public interface IFileService
    {
        Task SetAvatarAsync(string userGuid, IFormFile file);
    }
}