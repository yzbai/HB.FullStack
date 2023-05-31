using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class DirectoryTokenRes<TId> : SharedResource2<TId>
    {
        public TId UserId { get; set; } = default!;

        public string SecurityToken { get; set; } = null!;

        public string AccessKeyId { get; set; } = null!;

        public string AccessKeySecret { get; set; } = null!;

        /// <summary>
        /// 修正后的Directory,比如请求/a/b/c的权限，返回了/a的权限，即权限扩大
        /// </summary>
        public string DirectoryPermissionName { get; set; } = null!;

        public bool ReadOnly { get; set; }

        public override TId? Id { get; set; }

        public override long? ExpiredAt { get; set; }
    }
}