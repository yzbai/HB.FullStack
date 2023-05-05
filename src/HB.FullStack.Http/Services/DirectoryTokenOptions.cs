using System.Collections.Generic;

using HB.FullStack.Common.Files;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.WebLib.Services
{
    public class DirectoryTokenOptions : IOptions<DirectoryTokenOptions>
    {
        public DirectoryTokenOptions Value => this;

        public IList<DirectoryDescription> DirectoryDescriptions { get; set; } = new List<DirectoryDescription>();

        public IList<DirectoryPermission> DirectoryPermissions { get; set; } = new List<DirectoryPermission>();
    }
}
