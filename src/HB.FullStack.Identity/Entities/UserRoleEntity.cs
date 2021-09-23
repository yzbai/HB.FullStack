
using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    internal class UserRoleEntity : GuidEntity
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(UserEntity))]
        public Guid UserId { get; set; }

        [NoEmptyGuid]
        [ForeignKey(typeof(RoleEntity))]
        public Guid RoleId { get; set; }

        public UserRoleEntity() { }

        public UserRoleEntity(Guid userId, Guid roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }
    }
}
