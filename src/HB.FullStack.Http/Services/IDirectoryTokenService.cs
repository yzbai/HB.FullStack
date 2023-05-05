using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;


namespace HB.FullStack.Server.WebLib.Services
{
    public interface IDirectoryTokenService
    {
        Task<DirectoryToken?> GetDirectoryTokenAsync(Guid userId, string directoryPermissionName, string? regexPlaceHolderValue, bool readOnly, string lastUser);
    }
}
