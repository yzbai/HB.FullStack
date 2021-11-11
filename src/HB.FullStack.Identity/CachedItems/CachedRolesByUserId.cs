using HB.FullStack.Identity.Entities;
using HB.FullStack.Repository;
using HB.FullStack.Cache;

using Microsoft.Extensions.Caching.Distributed;

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace HB.FullStack.Identity
{
    /// <summary>
    /// userid: Roles
    /// 关联实体：UserRole, Role
    /// 典型的三表关系
    /// 当UserRole变化时，要Invalidate 对应key的条目
    /// 当Role变化，要Invalidate所有的条目
    /// </summary>
    internal class CachedRolesByUserId : CachedItem<IEnumerable<Role>>
    {
        public CachedRolesByUserId(Guid userId) : base(userId) { }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(30);
    }
}
