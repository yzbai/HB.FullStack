using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using HB.FullStack.Common.Models;
using HB.FullStack.Database.Config;
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

        #region Environment
        
        public DbEngineType EngineType { get; set; }

        public IDbEngine Engine { get; set; } = null!;

        public DbSchema DbSchema { get; set; } = null!;

        public string TableName { get; set; } = null!;

        #endregion

        public DbModelIdType IdType { get; set; } = DbModelIdType.Unkown;

        public bool IsTimestamp { get; set; }

        public DbConflictCheckMethods AllowedConflictCheckMethods { get; set; } = DbConflictCheckMethods.OldNewValueCompare | DbConflictCheckMethods.Timestamp;

        public DbConflictCheckMethods BestConflictCheckMethodWhenUpdate { get; set; }

        public DbConflictCheckMethods BestConflictCheckMethodWhenDelete { get; set; }

        /// <summary>
        /// 数据库是否可写
        /// </summary>
        public bool IsWriteable { get; set; }

        public int FieldCount { get; set; }

        public int UniqueFieldCount { get; set; }

        public DbModelPropertyDef PrimaryKeyPropertyDef { get; internal set; } = null!;
        
        public DbModelPropertyDef DeletedPropertyDef { get; internal set; } = null!;

        public DbModelPropertyDef LastUserPropertyDef { get; internal set; } = null!;

        [NotNullIfNotNull(nameof(IsTimestamp))]
        public DbModelPropertyDef? TimestampPropertyDef { get; internal set; } = null!;

        private IList<DbModelPropertyDef>? _foreignKeyProperties;
        private string? _dbTableReservedName;
        
        public string DbTableReservedName => _dbTableReservedName ??= SqlHelper.GetReserved(TableName, EngineType);

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

        public ConnectionString MasterConnectionString => DbSchema.GetMasterConnectionString();

        public ConnectionString SlaverConnectionString => DbSchema.GetSlaveConnectionString();

        public override ModelPropertyDef? GetPropertyDef(string propertyName) => GetDbPropertyDef(propertyName);
    }

    public static class DbModelDefExtensions
    {
        public static DbModelDef ThrowIfNotWriteable(this DbModelDef modelDef)
        {
            if (!modelDef.IsWriteable)
            {
                throw DbExceptions.NotWriteable(type: modelDef.FullName, database: modelDef.DbSchema.Name);
            }

            return modelDef;
        }

        public static DbModelDef ThrowIfNotTimestamp(this DbModelDef modelDef)
        {
            if (!modelDef.IsTimestamp)
            {
                throw DbExceptions.ConflictCheckError($"{modelDef.FullName} is not ITimestamp, but update properties using timestamp check.");
            }

            return modelDef;
        }
    }
}