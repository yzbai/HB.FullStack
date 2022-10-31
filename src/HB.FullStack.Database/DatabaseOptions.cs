using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Options;

namespace HB.FullStack.Database
{
    public class DatabaseOptions : IOptions<DatabaseOptions>
    {
        public DatabaseOptions Value => this;

        /// <summary>
        /// 有哪些数据库
        /// 由DbManager处理
        /// </summary>
        public IList<DbSetting> DbSettings { get; set; } = new List<DbSetting>();

        /// <summary>
        /// 指定包含DbModel的Assemblies，不再去其他Assembly中查找
        /// </summary>
        public IList<string> DbModelAssemblies { get; set; } = new List<string>();

        /// <summary>
        /// 有哪些DbModel
        /// 由DbModelDefFactory处理
        /// </summary>
        public IList<DbModelSetting> DbModelSettings { get; set; } = new List<DbModelSetting>();
    }

    /// <summary>
    /// 描述一个Database
    /// </summary>
    public class DbSetting
    {
        /// <summary>
        /// 支持不同数据库混用
        /// </summary>
        public EngineType EngineType { get; set; }

        /// <summary>
        /// 为了实现数据库切换，比如不同用户登录，使用不同数据库
        /// 动态数据库
        /// </summary>
        public string DbSchema { get; set; } = null!;

        /// <summary>
        /// 从1开始
        /// </summary>
        public int Version { get; set; }

        //TODO: 确保useAffectedRows=false
        /// <summary>
        /// 可能在初始化才确定
        /// </summary>
        public ConnectionString? ConnectionString { get; set; }

        public IList<ConnectionString>? SlaveConnectionStrings { get; set; }


        #region Other Settings

        public string? TableNameSuffixToRemove { get; set; } = "Model";

        public int DefaultVarcharLength { get; set; } = 200;

        public bool AutomaticCreateTable { get; set; } = true;

        public bool AddDropStatementWhenCreateTable { get; set; }

        public int MaxBatchNumber { get; set; } = 500;

        /// <summary>
        /// 默认的是否真正删除
        /// </summary>
        public bool DefaultTrulyDelete { get; set; } = false;

        #endregion

    }

    /// <summary>
    /// 描述一个DbMoel
    /// </summary>
    public class DbModelSetting
    {
        [DisallowNull, NotNull]
        public string ModelFullName { get; set; } = null!;

        ///// <summary>
        ///// 有DbName按DbName为准，没有按DbKind来
        ///// </summary>
        //public string? DbName { get; set; }

        //public string? DbKind { get; set; }

        public DbSchema DbSchema { get; set; } = null!;

        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        public bool? ReadOnly { get; set; }
    }
}