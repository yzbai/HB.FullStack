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
        /// 不具体指定某一个数据，而是指定某种数据库Schema。
        /// 具体的某一个数据库由ConnectionString来指定
        /// </summary>
        public string DbSchema { get; set; } = null!;

        /// <summary>        
        /// 支持不同数据库混用
        /// </summary>
        public EngineType EngineType { get; set; }

        /// <summary>
        /// 从1开始
        /// </summary>
        public int Version { get; set; }

        
        /// <summary>
        /// 具体指定某一 个数据库
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

        public DbSchema DbSchema { get; set; } = null!;

        [DisallowNull, NotNull]
        public string TableName { get; set; } = null!;

        public bool? ReadOnly { get; set; }
    }
}