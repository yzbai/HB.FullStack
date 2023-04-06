
using System;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Database.Config;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Implements;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;

namespace HB.FullStack.Database
{
    /// <summary>
    /// 实现单表的数据库与内存映射
    /// 数据库 Write/Read Controller
    /// 要求：每张表必须有一个主键，且主键必须为int。
    /// 异常处理设置：DAL层处理DbException,其他Exception直接扔出。每个数据库执行者，只扔出异常。
    /// 异常处理，只用在写操作上。
    /// 乐观锁用在写操作上，交由各个数据库执行者实施，Version方式。
    /// 批量操作，采用事务方式，也交由各个数据库执行者实施。
    /// </summary>
    internal sealed partial class DefaultDatabase : IDatabase
    {
        private readonly ILogger _logger;

        private IDbSchemaManager _dbSchemaManager { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public IDbCommandBuilder DbCommandBuilder { get; }

        public ITransaction Transaction { get; }

        public DefaultDatabase(
            ILogger<DefaultDatabase> logger,
            IDbSchemaManager dbSchemaManager,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction)
        {
            _logger = logger;
            _dbSchemaManager = dbSchemaManager;

            ModelDefFactory = modelDefFactory;
            DbCommandBuilder = commandBuilder;
            Transaction = transaction;
        }

        #region SystemInfo 管理

        private async Task<SystemInfo?> GetSystemInfoAsync(DbSchema dbSchema, TransactionContext transContext)
        {
            bool isExisted = await IsTableExistsAsync(dbSchema, SystemInfoNames.SYSTEM_INFO_TABLE_NAME, transContext).ConfigureAwait(false);

            if (!isExisted)
            {
                return null;
            }

            var command = DbCommandBuilder.CreateSystemInfoRetrieveCommand(dbSchema.EngineType);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            using IDataReader reader = await engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false);

            SystemInfo systemInfo = new SystemInfo(dbSchema.Name);

            while (reader.Read())
            {
                systemInfo.Set(reader["Name"].ToString()!, reader["Value"].ToString()!);
            }

            return systemInfo;
        }

        private async Task SetSystemVersionAsync(int version, DbSchema dbSchema, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateSystemVersionSetCommand(dbSchema.EngineType, dbSchema.Name, version);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        #endregion

        #region Table 管理

        public async Task<bool> IsTableExistsAsync(DbSchema dbSchema, string tableName, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateIsTableExistCommand(dbSchema.EngineType, tableName);

            var engine = _dbSchemaManager.GetDatabaseEngine(dbSchema.EngineType);

            object? result = await engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false);

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        private async Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, DbSchema dbSchema)
        {
            var engine = _dbSchemaManager.GetDatabaseEngine(def.EngineType);

            var command = DbCommandBuilder.CreateTableCreateCommand(
                def,
                dbSchema.AddDropStatementWhenCreateTable,
                dbSchema.DefaultVarcharFieldLength,
                dbSchema.MaxVarcharFieldLength,
                dbSchema.MaxMediumTextFieldLength);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return await engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        private async Task CreateTablesByDbSchemaAsync(DbSchema dbSchema, TransactionContext trans)
        {
            foreach (DbModelDef modelDef in ModelDefFactory.GetAllDefsByDbSchema(dbSchema.Name))
            {
                await CreateTableAsync(modelDef, trans, dbSchema).ConfigureAwait(false);
            }
        }

        #endregion

        #region 条件构造

        public FromExpression<T> From<T>() where T : DbModel, new() => DbCommandBuilder.From<T>();

        public WhereExpression<T> Where<T>() where T : DbModel, new() => DbCommandBuilder.Where<T>();

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new() => DbCommandBuilder.Where<T>(sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new() => DbCommandBuilder.Where(predicate);

        #endregion
    }
}