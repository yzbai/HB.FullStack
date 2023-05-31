using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;

namespace HB.FullStack.Server.WebLib.Services
{
    public class DirectoryToken<TId> : IExpired
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

        public long? ExpiredAt { get; set; }
    }
}
