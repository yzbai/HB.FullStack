using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    public class Role : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [EntityProperty("角色名", Unique = true, NotNull = true)]
        public string Name { get; set; }

        [EntityProperty("DisplayName", Length=500)]
        public string DisplayName { get; set; }

        [EntityProperty("是否激活")]
        public bool IsActivated { get; set; }

        [EntityProperty("说明", Length=1024)]
        public string Comment { get; set; }
    }

    
}