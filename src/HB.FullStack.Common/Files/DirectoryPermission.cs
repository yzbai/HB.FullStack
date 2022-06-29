using System;
using System.Collections.Generic;

namespace HB.FullStack.Common.Files
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "<Pending>")]
    public class DirectoryPermission
    {
        public string PermissionName { get; set; } = null!;

        /// <summary>
        /// 这个权限下的顶层Directory
        /// </summary>
        public string TopDirectory { get; set; } = null!;

        public bool ReadOnly { get; set; } = true;

        public IList<string> AllowedUserLevels { get; set; } = new List<string>();

        /// <summary>
        /// 服务器端根据此来设定Sts的有效期
        /// </summary>
        public TimeSpan ExpiryTime { get; set; } = TimeSpan.FromHours(1);

        public bool ContainsPlaceHoder { get; set; }

        public string? PlaceHolderName { get; set; }

        public bool IsUserPrivate { get; set; }
    }
}
