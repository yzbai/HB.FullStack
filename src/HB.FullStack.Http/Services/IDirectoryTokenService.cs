using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Shared;


namespace HB.FullStack.Server.WebLib.Services
{
    public interface IDirectoryTokenService<TId>
    {
        DirectoryToken<TId>? GetDirectoryToken(TId requestUserId, string? userLevel, string directoryPermissionName, string? placeHolderValue, bool readOnly);
    }
}
