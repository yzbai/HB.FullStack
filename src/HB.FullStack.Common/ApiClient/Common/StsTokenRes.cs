using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public class StsTokenRes : GuidResource
    {
        public Guid UserId { get; set; }

        public string SecurityToken { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        public DateTimeOffset ExpirationAt { get; set; }

        /// <summary>
        /// 修正后的Directory,比如请求/a/b/c的权限，返回了/a的权限，即权限扩大
        /// </summary>
        public string DirectoryPermissionName { get; set; } = null!;

        public bool ReadOnly { get; set; }

        protected override int GetChildHashCode()
        {
            return HashCode.Combine(UserId, SecurityToken, AccessKeyId, AccessKeySecret, ExpirationAt, DirectoryPermissionName, ReadOnly);
        }
    }

}
