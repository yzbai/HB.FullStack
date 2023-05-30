using System;
using System.Collections.Generic;

using HB.FullStack.Cache;
using HB.FullStack.Server.Identity.Models;

namespace HB.FullStack.Server.Identity
{
    /// <summary>
    /// userid: Roles
    /// 关联实体：UserRole, Role
    /// 典型的三表关系
    /// 当UserRole变化时，要Invalidate 对应key的条目
    /// 当Role变化，要Invalidate所有的条目
    /// </summary>
    internal class CachedRolesByUserId<TId> : CachedItem<IEnumerable<Role<TId>>>
    {
        public CachedRolesByUserId(TId userId) : base(userId) { }

        public override TimeSpan? AbsoluteExpirationRelativeToNow => null;

        public override TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(30);

        public override string WhenToInvalidate => "当UserRole变化时，要Invalidate 对应key的条目;当Role变化，要Invalidate所有的条目";
    }
}
