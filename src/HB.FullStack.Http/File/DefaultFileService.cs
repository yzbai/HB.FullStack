using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Server;
using HB.FullStack.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.File
{
    public class DefaultFileService : IFileService
    {
        private readonly ServerOptions _siteOptions;
        private readonly ISecurityService _securityService;

        public DefaultFileService(IOptions<ServerOptions> options, ISecurityService securityService)
        {
            _siteOptions = options.Value;
            _securityService = securityService;
        }

        public async Task SetAvatarAsync(long userId, IFormFile file)
        {
            byte[] data = await _securityService.ProcessFormFileAsync(
                    file,
                    new string[] { ".png" },
                    _siteOptions.FileSettings.AvatarMaxSize).ConfigureAwait(false);


            string path = Path.Combine(_siteOptions.FileSettings.AvatarPath, userId + ".png");

            using FileStream fileStream = new FileStream(path, FileMode.Create);

            await fileStream.WriteAsync(data).ConfigureAwait(false);
        }
    }
}
