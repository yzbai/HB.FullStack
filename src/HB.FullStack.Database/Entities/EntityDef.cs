using System;
using System.Collections.Generic;

namespace HB.FullStack.Database.Def
{
    /// <summary>
    /// 实体定义
    /// </summary>
    internal class EntityDef
    {
        public bool IsIdAutoIncrement { get; set; }

        public bool IsIdGuid { get; set; }
        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityFullName { get; set; } = null!;

        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; set; } = null!;

        /// <summary>
        /// 所属数据库
        /// 在IsTableModel为false时没有意义
        /// </summary>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// 数据库是否可写
        /// </summary>
        public bool DatabaseWriteable { get; set; }

        /// <summary>
        /// 数据库表名
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string TableName { get; set; } = null!;

        /// <summary>
        /// 数据库专有化后的名称
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableReservedName { get; set; } = null!;

        public int FieldCount { get; set; }

        public int UniqueFieldCount { get; set; }

        /// <summary>
        /// 属性字典
        /// </summary>
        public Dictionary<string, EntityPropertyDef> PropertyDict { get; } = new Dictionary<string, EntityPropertyDef>();

        /// <summary>
        /// 属性枚举
        /// </summary>
        public List<EntityPropertyDef> PropertyDefs { get; } = new List<EntityPropertyDef>();

        public EntityPropertyDef PrimaryKeyPropertyDef { get; internal set; } = null!;

        public EntityPropertyDef? GetPropertyDef(string propertyName)
        {
            if (PropertyDict.ContainsKey(propertyName))
            {
                return PropertyDict[propertyName];
            }

            return null;
        }
    }
}