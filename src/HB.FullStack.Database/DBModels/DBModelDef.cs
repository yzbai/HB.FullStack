using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Models;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

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

        public DbSchema DbSchema { get; set; } = null!; 

        /// <summary>
        /// 属于那种数据库
        /// </summary>
        public EngineType EngineType { get; set; }

        /// <summary>
        /// 数据库表名
        /// 在IsTableDomain为false时没有意义
        /// </summary>
        public string TableName { get; set; } = null!;

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
        /// 数据库是否可写
        /// </summary>
        public bool DbWriteable { get; set; }

        public int FieldCount { get; set; }

        public int UniqueFieldCount { get; set; }

        public DbModelPropertyDef PrimaryKeyPropertyDef { get; internal set; } = null!;

        private string? _dbTableReservedName;
        private string? _deletedPropertyReservedName;
        private IList<DbModelPropertyDef>? _foreignKeyProperties;

        public string DbTableReservedName => _dbTableReservedName ??= SqlHelper.GetReserved(TableName, EngineType);

        public string DeletedPropertyReservedName => _deletedPropertyReservedName ??= SqlHelper.GetReserved(nameof(DbModel.Deleted), EngineType);

        public IList<DbModelPropertyDef> ForeignKeyProperties => _foreignKeyProperties ??= PropertyDict.Values.Where(p => p.IsForeignKey).ToList();


        public DbModelPropertyDef? GetDbPropertyDef(string propertyName)
        {
            if (PropertyDict.TryGetValue(propertyName, out DbModelPropertyDef? propertyDef))
            {
                return propertyDef;
            }

            return null;
        }

        public bool ContainsProperty(string propertyName) => PropertyDict.ContainsKey(propertyName);


        public override ModelPropertyDef? GetPropertyDef(string propertyName) => GetDbPropertyDef(propertyName);
    }
}