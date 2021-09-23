
using HB.FullStack.Common;
using HB.FullStack.Database.Entities;

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.ModelObjects
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class UserRole : ModelObject
    {
        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [NoEmptyGuid]
        public Guid RoleId { get; set; }
    }
}
