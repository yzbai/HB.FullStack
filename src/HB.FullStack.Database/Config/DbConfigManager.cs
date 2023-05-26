using DbSchemaName = System.String;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using HB.FullStack.Database.Engine;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace HB.FullStack.Database.Config
{
    //TODO: Db HealthCheck
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    //TODO: 记录Settings到tb_sys_info表中，自动加载
    //TODO: 处理slave库的brandNewCreate，以及Migration
    internal class DbConfigManager : IDbConfigManager
    {
        private readonly DbOptions _options;
        private readonly Dictionary<DbSchemaName, DbSchema> _dbSchemaDict = new Dictionary<DbSchemaName, DbSchema>();

        private readonly IDbEngine? _sqliteEngine;
        private readonly IDbEngine? _mysqlEngine;
        private readonly IEnumerable<DbInitContext>? _initContexts;

        public DbConfigManager(IOptions<DbOptions> options, IEnumerable<IDbEngine> dbEngines, IEnumerable<DbInitContext>? initContexts)
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

                schema.EnsureUseAffectedRowsIsFalse();

                if (schema.IsDefault && DefaultDbSchema == null)
                {
                    DefaultDbSchema = schema;
                }

                schema.Engine = GetDbEngine(schema.EngineType);

                _dbSchemaDict[schema.Name] = schema;
            }

            DefaultDbSchema ??= _options.DbSchemas[0];
            _initContexts = initContexts;
        }

        public IEnumerable<DbInitContext>? InitContexts => _initContexts;

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


    }
}
