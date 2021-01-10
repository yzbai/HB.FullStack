using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace HB.FullStack.Server.File
{
    public interface IFileService
    {
        /// <exception cref="ApiException"></exception>
        Task SetAvatarAsync(long userId, IFormFile file);
    }
}