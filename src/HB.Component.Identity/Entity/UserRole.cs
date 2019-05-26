using HB.Framework.Database.Entity;
using System;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 用户-角色 关系 实体
    /// </summary>
    public class UserRole : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [ForeignKey(typeof(User))]
        [GuidEntityProperty]
        public string UserGuid { get; set; }


        [ForeignKey(typeof(Role))]
        [GuidEntityProperty]
        public string RoleGuid { get; set; }
    }
}
