using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Entities;

using System;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Identity.Entities
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    [DatabaseEntity]
    public class RoleOfUser : Entity
    {
        [ForeignKey(typeof(User))]
        [GuidEntityProperty(NotNull = true)]
        [DisallowNull, NotNull]
        public string UserGuid { get; set; } = default!;


        [ForeignKey(typeof(Role))]
        [GuidEntityProperty(NotNull = true)]
        public string RoleGuid { get; set; } = default!;
    }
}
