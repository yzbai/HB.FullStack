using DbSchemaName = System.String;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HB.FullStack.Database.Engine;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace HB.FullStack.Database.Config
{
    //TODO: Db HealthCheck
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    //TODO: 记录Settings到tb_sys_info表中，自动加载
    //TODO: 处理slave库的brandNewCreate，以及Migration
    //TODO: 确保mysql中useAffectedRows=false
    internal class DbSchemaManager : IDbSchemaManager
    {
        /// <summary>
        ///  Including some statistics info
        /// </summary>
        class DbSchemaEx
        {
            public ushort SlaveAccessCount = 0;
            public int SlaveCount;

            public DbSchema Schema;

            public DbSchemaEx(DbSchema schema)
            {
                Schema = schema;
                SlaveCount = schema.SlaveConnectionStrings == null ? 0 : schema.SlaveConnectionStrings.Count;
            }
        }

        private readonly IDbEngine? _mysqlEngine;
        private readonly IDbEngine? _sqliteEngine;
        private readonly Dictionary<DbSchemaName, DbSchemaEx> _dbSchemaExDict = new Dictionary<DbSchemaName, DbSchemaEx>();

        public DbSchemaManager(IOptions<DbOptions> options, IEnumerable<IDbEngine> databaseEngines)
        {
            DbOptions _options = options.Value;

            //Range DbSchema
            foreach (DbSchema schema in _options.DbSchemas)
            {
                if (schema.Version < 1)
                {
                    throw DbExceptions.DbSchemaError(schema.Version, schema.Name, "DbSchema Version must bigger than 0");
                }

                if (schema.Name.IsNullOrEmpty())
                {
                    throw DbExceptions.DbSchemaError(schema.Version, schema.Name, "DbSchema Name Can not be null or empty");
                }

                _dbSchemaExDict[schema.Name] = new DbSchemaEx(schema);
            }

            //Range DatabaseEngines
            foreach (IDbEngine engine in databaseEngines)
            {
                if (engine.EngineType == DbEngineType.SQLite)
                {
                    _sqliteEngine = engine;
                }
                else if (engine.EngineType == DbEngineType.MySQL)
                {
                    _mysqlEngine = engine;
                }
            }
        }

        public IList<DbSchema> GetAllDbSchemas()
        {
            return _dbSchemaExDict.Values.Select(v => v.Schema).ToList();
        }

        public DbSchema GetDbSchema(string dbSchemaName)
        {
            return _dbSchemaExDict[dbSchemaName].Schema;
        }

        public void SetConnectionString(string dbSchemaName, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            DbSchemaEx unit = _dbSchemaExDict[dbSchemaName];

            if (connectionString.IsNotNullOrEmpty())
            {
                unit.Schema.ConnectionString = new ConnectionString(connectionString.ThrowIfNullOrEmpty($"在初始化时，应该为 {dbSchemaName} 提供连接字符串"));
            }

            if (slaveConnectionStrings != null)
            {
                unit.Schema.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
                unit.SlaveCount = slaveConnectionStrings.Count;
            }
        }

        public ConnectionString GetConnectionString(string dbSchemaName, bool useMaster)
        {
            DbSchemaEx unit = _dbSchemaExDict[dbSchemaName];

            return useMaster
                ? unit.Schema.ConnectionString.ThrowIfNull($"{dbSchemaName} 没有ConnectionString")
                : GetSlaveConnectionString(unit);

            static ConnectionString GetSlaveConnectionString(DbSchemaEx dbUnit)
            {
                //这里采取平均轮训的方法
                if (dbUnit.SlaveCount == 0)
                {
                    return dbUnit.Schema.ConnectionString.ThrowIfNull($"{dbUnit.Schema.Name} 没有ConnectionString");
                }

                return dbUnit.Schema.SlaveConnectionStrings![dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
            }
        }

        public IDbEngine GetDatabaseEngine(string dbSchemaName) => GetDatabaseEngine(_dbSchemaExDict[dbSchemaName].Schema.EngineType);

        public IDbEngine GetDatabaseEngine(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                DbEngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
