using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;
using HB.FullStack.Server.Services;
using HB.Infrastructure.Aliyun.Sts;

namespace HB.FullStack.Server.WebLib.Services
{
    public class DirectoryTokenService : IDirectoryTokenService
    {
        private readonly IAliyunStsService _aliyunStsService;

        public DirectoryTokenService(IAliyunStsService aliyunStsService)
        {
            _aliyunStsService = aliyunStsService;
        }

        public Task<DirectoryToken?> GetDirectoryTokenAsync(Guid userId, string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly, string lastUser)
        {
            throw new NotImplementedException();
        }
    }
}
