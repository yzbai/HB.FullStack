using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 实体定义
    /// </summary>
    public class DbModelDef : ModelDef
    {
        #region Common ModelDef

        /// <summary>
        /// 属性字典
        /// </summary>
        public Dictionary<string, DbModelPropertyDef> PropertyDict { get; } = new Dictionary<string, DbModelPropertyDef>();

        /// <summary>
        /// 属性枚举
        /// </summary>
        public IList<DbModelPropertyDef> PropertyDefs { get; } = new List<DbModelPropertyDef>();

        #endregion

        public bool IsTimestampDBModel { get; set; }

        /// <summary>
        /// 是否是GuidModel
        /// </summary>
        public bool IsIdGuid { get; set; }

        /// <summary>
        /// 是否是IdLongModel
        /// </summary>
        public bool IsIdLong { get; set; }

        /// <summary>
        /// 是否是AutoincrementIdModel
        /// </summary>
        public bool IsIdAutoIncrement { get; set; }

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

        public DbModelPropertyDef PrimaryKeyPropertyDef { get; internal set; } = null!;

        public DbModelPropertyDef? GetDbPropertyDef(string propertyName)
        {
            if (PropertyDict.TryGetValue(propertyName, out DbModelPropertyDef? propertyDef))
            {
                return propertyDef;
            }

            return null;

            //if (PropertyDict.ContainsKey(propertyName))
            //{
            //    return PropertyDict[propertyName];
            //}

            //return null;
        }

        public bool ContainsProperty(string propertyName)
        {
            return PropertyDict.ContainsKey(propertyName);
        }

        private IList<DbModelPropertyDef>? _foreignKeyProperties;

        public IList<DbModelPropertyDef> GetForeignKeyProperties()
        {
            return _foreignKeyProperties ??= PropertyDict.Values.Where(p => p.IsForeignKey).ToList();
        }

        public override ModelPropertyDef? GetPropertyDef(string propertyName) => GetDbPropertyDef(propertyName);
    }
}