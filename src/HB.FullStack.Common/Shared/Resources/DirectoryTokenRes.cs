using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public class DirectoryTokenRes : ISharedResource
    {
         object UserId { get; set; }

         string SecurityToken { get; set; }

         string AccessKeyId { get; set; }

         string AccessKeySecret { get; set; }

        /// <summary>
        /// 修正后的Directory,比如请求/a/b/c的权限，返回了/a的权限，即权限扩大
        /// </summary>
         string DirectoryPermissionName { get; set; }

         bool ReadOnly { get; set; }


        //protected override int GetChildHashCode()
        //{
        //    return HashCode.Combine(UserId, SecurityToken, AccessKeyId, AccessKeySecret, ExpirationAt, DirectoryPermissionName, ReadOnly);
        //}
    }
}