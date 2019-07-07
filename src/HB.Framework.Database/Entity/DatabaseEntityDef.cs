using System;
using System.Collections.Generic;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 实体定义
    /// </summary>
    internal class DatabaseEntityDef
    {
        #region 自身描述

        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityFullName { get; set; }
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType { get; set; }

        #endregion

        #region 针对数据库描述
        /// <summary>
        /// 是否是TableDomain
        /// </summary>
        public bool IsTableModel { get; set; }
        /// <summary>
        /// 所属数据库
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// 数据库是否可写
        /// </summary>
        public bool DatabaseWriteable { get; set; }

        /// <summary>
        /// 数据库表名
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// 数据库专有化后的名称
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableReservedName { get; set; }

        /// <summary>
        /// 数据库表描述（备注）
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableDescription { get; set; }

        public int FieldCount { get; set; }

        #endregion

        #region 所含属性

        /// <summary>
        /// 属性字典
        /// </summary>
        public Dictionary<string, DatabaseEntityPropertyDef> PropertyDict { get; } = new Dictionary<string, DatabaseEntityPropertyDef>();
        /// <summary>
        /// 属性枚举
        /// </summary>
        public IEnumerable<DatabaseEntityPropertyDef> Properties { get { return PropertyDict.Values; } }

        #endregion

        public DatabaseEntityDef()
        {
            FieldCount = 0;
        }

        public DatabaseEntityPropertyDef GetProperty(string propertyName)
        {
            if (PropertyDict.ContainsKey(propertyName))
            {
                return PropertyDict[propertyName];
            }

            return null;
        }
    }


}

