using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.FullStack.Common.Shared.Resources;

namespace HB.FullStack.Server.Services
{
    public interface IStsTokenService
    {
        Task<StsTokenRes?> GetAliyunOssStsTokenAsync(Guid userId, string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly, string lastUser);
    }
}
