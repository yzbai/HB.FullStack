
using HB.FullStack.Database.Entities;

using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class RoleOfUser : IdGenEntity
    {
        [LongId]
        [ForeignKey(typeof(User))]
        public long UserId { get; set; }


        [LongId]
        [ForeignKey(typeof(Role))]
        public long RoleId { get; set; }

        public RoleOfUser() { }

        public RoleOfUser(long userId, long roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
