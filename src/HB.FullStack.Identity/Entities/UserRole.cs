using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>

    public class UserRole : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        [MessagePack.Key(7)]
        public Guid UserId { get; set; }

        [NoEmptyGuid]
        [ForeignKey(typeof(Role), false)]
        [MessagePack.Key(8)]
        public Guid RoleId { get; set; }

        public UserRole()
        {
        }

        public UserRole(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}