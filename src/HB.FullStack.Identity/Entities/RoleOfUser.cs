
using HB.FullStack.Database.Def;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class RoleOfUser : IdGenEntity
    {
        [Required]
        [ForeignKey(typeof(User))]
        [EntityProperty(NotNull = true)]
        public long UserId { get; set; }


        [Required]
        [ForeignKey(typeof(Role))]
        [EntityProperty(NotNull = true)]
        public long RoleId { get; set; }

        public RoleOfUser() { }

        public RoleOfUser(long userId, long roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
