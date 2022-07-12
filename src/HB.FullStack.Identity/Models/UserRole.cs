using HB.FullStack.Database.DatabaseModels;

using System;
using System.ComponentModel.DataAnnotations;

namespace HB.FullStack.Identity.Models
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class UserRole : GuidDatabaseModel
    {
        [NoEmptyGuid]
        [ForeignKey(typeof(User), false)]
        public Guid UserId { get; set; }

        [NoEmptyGuid]
        [ForeignKey(typeof(Role), false)]
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