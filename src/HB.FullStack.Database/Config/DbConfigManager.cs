using DbSchemaName = System.String;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HB.FullStack.Database.Engine;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Database.Config
{
    //TODO: Db HealthCheck
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    //TODO: 记录Settings到tb_sys_info表中，自动加载
    //TODO: 处理slave库的brandNewCreate，以及Migration
    //TODO: 确保mysql中useAffectedRows=false
    internal class DbConfigManager : IDbConfigManager
    {
        private DbOptions _options;
        private readonly Dictionary<DbSchemaName, DbSchema> _dbSchemaDict = new Dictionary<DbSchemaName, DbSchema>();

        private IDbEngine? _sqliteEngine;
        private IDbEngine? _mysqlEngine;

        public DbConfigManager(IOptions<DbOptions> options, IEnumerable<IDbEngine> dbEngines)
        {
            _options = options.Value;

            //Range DbEngines
            foreach (IDbEngine engine in dbEngines)
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

                if (schema.IsDefault && DefaultDbSchema == null)
                {
                    DefaultDbSchema = schema;
                }

                schema.DbEngine = GetDbEngine(schema.EngineType);

                _dbSchemaDict[schema.Name] = schema;
            }

            DefaultDbSchema ??= _options.DbSchemas[0];
        }

        public IDbEngine GetDbEngine(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                DbEngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }

        public DbSchema GetDbSchema(string dbSchemaName)
        {
            return _dbSchemaDict[dbSchemaName];
        }

        public IList<string> DbModelAssemblies => _options.DbModelAssemblies;

        public DbSchema DefaultDbSchema { get; private set; }

        public IList<DbSchema> AllDbSchemas => _options.DbSchemas;

        public void SetConnectionString(string dbSchemaName, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            DbSchema schema = _dbSchemaDict[dbSchemaName];

            if (connectionString.IsNotNullOrEmpty())
            {
                schema.ConnectionString = new ConnectionString(connectionString.ThrowIfNullOrEmpty($"在初始化时，应该为 {dbSchemaName} 提供连接字符串"));
            }

            if (slaveConnectionStrings != null)
            {
                schema.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
            }
        }
    }

    public static class DbSchemaExtensions
    {
        private static Random _slaveConnectionRandom = new Random();

        public static ConnectionString GetSlaveConnectionString(this DbSchema dbSchema)
        {
            if (dbSchema.SlaveConnectionStrings.IsNullOrEmpty())
            {
                return dbSchema.ConnectionString.ThrowIfNull($"{dbSchema.Name} do not has master connection string.");
            }
            else
            {
                return dbSchema.SlaveConnectionStrings[_slaveConnectionRandom.Next() % dbSchema.SlaveConnectionStrings.Count];
            }

            //if (useMaster)
            //{
            //    return dbSchema.ConnectionString;
            //}

            //DbSchemaEx unit = _dbSchemaExDict[dbSchema.Name];

            //return GetSlaveConnectionString(unit);

            //static ConnectionString? GetSlaveConnectionString(DbSchemaEx dbUnit)
            //{
            //    //这里采取平均轮训的方法
            //    if (dbUnit.SlaveCount == 0)
            //    {
            //        return dbUnit.Schema.ConnectionString;
            //    }

            //    return dbUnit.Schema.SlaveConnectionStrings![dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
            //}
        }

        public static ConnectionString GetMasterConnectionString(this DbSchema dbSchema)
        {
            return dbSchema.ConnectionString.ThrowIfNull($"{dbSchema.Name} do not has master connection string.");
        }
    }
}
