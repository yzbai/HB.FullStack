using System;
using System.Collections.Generic;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 实体定义
    /// </summary>
    public class DatabaseEntityDef
    {
        #region 自身描述

        /// <summary>
        /// 实体名
        /// </summary>
        public string EntityFullName;
        /// <summary>
        /// 实体类型
        /// </summary>
        public Type EntityType;
               
        #endregion

        #region 针对数据库描述
        /// <summary>
        /// 是否是TableDomain
        /// </summary>
        public bool IsTableModel;
        /// <summary>
        /// 所属数据库
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DatabaseName;

        /// <summary>
        /// 数据库是否可写
        /// </summary>
        public bool DatabaseWriteable;

        /// <summary>
        /// 数据库表名
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string TableName;
        /// <summary>
        /// 数据库专有化后的名称
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableReservedName;

        /// <summary>
        /// 数据库表描述（备注）
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string DbTableDescription;

        public int FieldCount;
        
        #endregion

        #region 所含属性

        /// <summary>
        /// 属性字典
        /// </summary>
        public Dictionary<string, DatabaseEntityPropertyDef> PropertyDict;

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

