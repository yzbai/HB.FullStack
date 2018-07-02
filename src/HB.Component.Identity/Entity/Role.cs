using HB.Framework.Database.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace HB.Component.Identity.Entity
{
    /// <summary>
    /// 角色
    /// </summary>
    [Serializable]
    public class Role : DatabaseEntity
    {
        [Required]
        [DatabaseEntityProperty("角色名", Unique = true, NotNull = true)]
        public string Name { get; set; }

        [DatabaseEntityProperty("DisplayName", Length=500)]
        public string DisplayName { get; set; }

        [DatabaseEntityProperty("是否激活")]
        public bool IsActivated { get; set; }

        [DatabaseEntityProperty("说明", Length=1024)]
        public string Comment { get; set; }
    }

    
}