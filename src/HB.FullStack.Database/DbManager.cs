global using DbSchema = System.String;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace HB.FullStack.Database
{
    //TODO: 增加数据库坏掉，自动切换, 比如屏蔽某个Slave，或者master切换到slave
    //TODO: 记录Settings到tb_sys_info表中，自动加载
    //TODO: 处理slave库的brandNewCreate，以及Migration
    //TODO: 确保mysql中useAffectedRows=false
    internal class DbSettingManager : IDbSettingManager
    {
        class DbManageUnit
        {
            public ushort SlaveAccessCount = 0;
            public int SlaveCount;

            public DbSetting Setting;

            public DbManageUnit(DbSetting setting)
            {
                Setting = setting;
                SlaveCount = setting.SlaveConnectionStrings == null ? 0 : setting.SlaveConnectionStrings.Count;
            }
        }

        private readonly IEnumerable<IDatabaseEngine> _databaseEngines;
        private readonly IDatabaseEngine? _mysqlEngine;
        private readonly IDatabaseEngine? _sqliteEngine;
        private readonly Dictionary<DbSchema, DbManageUnit> _dbManageUnits = new Dictionary<string, DbManageUnit>();

        public DbSettingManager(IOptions<DatabaseOptions> options, IEnumerable<IDatabaseEngine> databaseEngines)
        {
            DatabaseOptions _options = options.Value;
            _databaseEngines = databaseEngines;

            //Range DbSettings
            foreach (DbSetting dbSetting in _options.DbSettings)
            {
                if (dbSetting.Version < 0)
                {
                    throw DatabaseExceptions.DbSettingError(dbSetting.Version, dbSetting.DbSchema, "database Version must be 1");
                }

                if (dbSetting.DbSchema.IsNullOrEmpty())
                {
                    throw new ArgumentNullException("DbSetting中DbSchema不应该为空");
                }

                _dbManageUnits[dbSetting.DbSchema] = new DbManageUnit(dbSetting);
            }

            //Range DatabaseEngines
            foreach (IDatabaseEngine engine in _databaseEngines)
            {
                if (engine.EngineType == EngineType.SQLite)
                {
                    _sqliteEngine = engine;
                }
                else if (engine.EngineType == EngineType.MySQL)
                {
                    _mysqlEngine = engine;
                }
            }
        }

        #region Settings

        public void SetConnectionStringIfNeed(string dbSchema, string? connectionString, IList<string>? slaveConnectionStrings)
        {
            DbManageUnit unit = _dbManageUnits[dbSchema];

            //补充ConnectionString，不替换
            if (unit.Setting.ConnectionString == null)
            {
                unit.Setting.ConnectionString = new ConnectionString(connectionString.ThrowIfNullOrEmpty($"在初始化时，应该为 {unit.Setting.DbSchema} 提供连接字符串"));
            }

            //补充SlaveConnectionString,不替换
            if (unit.Setting.SlaveConnectionStrings == null && slaveConnectionStrings != null)
            {
                unit.Setting.SlaveConnectionStrings = slaveConnectionStrings.Select(c => new ConnectionString(c)).ToList();
                unit.SlaveCount = slaveConnectionStrings.Count;
            }
        }

        public ConnectionString GetConnectionString(DbSchema dbSchema, bool useMaster)
        {
            DbManageUnit unit = _dbManageUnits[dbSchema];

            return useMaster
                ? unit.Setting.ConnectionString.ThrowIfNull($"{unit.Setting.DbSchema} 没有ConnectionString")
                : GetSlaveConnectionString(unit);

            static ConnectionString GetSlaveConnectionString(DbManageUnit dbUnit)
            {
                //这里采取平均轮训的方法
                if (dbUnit.SlaveCount == 0)
                {
                    return dbUnit.Setting.ConnectionString.ThrowIfNull($"{dbUnit.Setting.DbSchema} 没有ConnectionString");
                }

                return dbUnit.Setting.SlaveConnectionStrings![dbUnit.SlaveAccessCount++ % dbUnit.SlaveCount];
            }
        }

        public IDatabaseEngine GetDatabaseEngine(DbSchema dbSchema) => GetDatabaseEngine(_dbManageUnits[dbSchema].Setting.EngineType);

        public IDatabaseEngine GetDatabaseEngine(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysqlEngine.ThrowIfNull("没有添加MySql"),
                EngineType.SQLite => _sqliteEngine.ThrowIfNull("没有添加Sqlite"),
                _ => throw new NotImplementedException(),
            };
        }

        public DbSetting GetDbSetting(string dbSchema)
        {
            return _dbManageUnits[dbSchema].Setting;
        }

        public int GetVarcharDefaultLength(DbSchema dbSchema)
        {
            int optionLength = _dbManageUnits[dbSchema].Setting.DefaultVarcharLength;

            return optionLength == 0 ? DefaultLengthConventions.DEFAULT_VARCHAR_LENGTH : optionLength;
        }

        public int GetMaxBatchNumber(DbSchema dbSchema)
        {
            return _dbManageUnits[dbSchema].Setting.MaxBatchNumber;
        }

        public bool GetDefaultTrulyDelete(DbSchema dbSchema)
        {
            return _dbManageUnits[dbSchema].Setting.DefaultTrulyDelete;
        }



        #endregion
    }
}
