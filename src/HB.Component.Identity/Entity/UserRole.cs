using HB.Framework.Database.Entity;
using System;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class UserRole : DatabaseEntity
    {
        [DatabaseForeignKey("用户ID", typeof(User))]
        public long UserId { get; set; }


        [DatabaseForeignKey("角色ID", typeof(Role))]
        public long RoleId { get; set; }
    }
}
