using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Server;
using HB.Framework.Server.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HB.Framework.Server.File
{
    public class DefaultFileService : IFileService
    {
        private readonly SiteOptions _siteOptions;
        private readonly ISecurityService _securityService;

        public DefaultFileService(IOptions<SiteOptions> options, ISecurityService securityService)
        {
            _siteOptions = options.Value;
            _securityService = securityService;
        }

        public async Task SetAvatarAsync(string userGuid, IFormFile file)
        {
            byte[] data = await _securityService.ProcessFormFileAsync(
                    file,
                    new string[] { ".png" },
                    _siteOptions.FileSettings.AvatarMaxSize).ConfigureAwait(false);


            string path = Path.Combine(_siteOptions.FileSettings.AvatarPath, userGuid! + ".png");

            using FileStream fileStream = new FileStream(path, FileMode.Create);

            await fileStream.WriteAsync(data).ConfigureAwait(false);
        }
    }
}
