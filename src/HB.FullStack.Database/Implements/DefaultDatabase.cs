
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using HB.FullStack.Common;
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

        private IDbConfigManager _dbConfigManager { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public IDbCommandBuilder DbCommandBuilder { get; }

        public ITransaction Transaction { get; }

        public DefaultDatabase(
            ILogger<DefaultDatabase> logger,
            IDbConfigManager dbConfigManager,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction)
        {
            _logger = logger;
            _dbConfigManager = dbConfigManager;

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

            using IDataReader reader = await dbSchema.Engine.ExecuteCommandReaderAsync(transContext.Transaction, command).ConfigureAwait(false);

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


            await dbSchema.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
        }

        #endregion

        #region Table 管理

        public async Task<bool> IsTableExistsAsync(DbSchema dbSchema, string tableName, TransactionContext transContext)
        {
            var command = DbCommandBuilder.CreateIsTableExistCommand(dbSchema.EngineType, tableName);

            object? result = await dbSchema.Engine.ExecuteCommandScalarAsync(transContext.Transaction, command).ConfigureAwait(false);

            return System.Convert.ToBoolean(result, Globals.Culture);
        }

        private async Task<int> CreateTableAsync(DbModelDef def, TransactionContext transContext, DbSchema dbSchema)
        {
            var command = DbCommandBuilder.CreateTableCreateCommand(
                def,
                dbSchema.AddDropStatementWhenCreateTable,
                dbSchema.DefaultVarcharFieldLength,
                dbSchema.MaxVarcharFieldLength,
                dbSchema.MaxMediumTextFieldLength);

            _logger.LogInformation("Table创建：{CommandText}", command.CommandText);

            return await dbSchema.Engine.ExecuteCommandNonQueryAsync(transContext.Transaction, command).ConfigureAwait(false);
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

        public FromExpression<T> From<T>() where T : BaseDbModel, new() => DbCommandBuilder.From<T>();

        public WhereExpression<T> Where<T>() where T : BaseDbModel, new() => DbCommandBuilder.Where<T>();

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : BaseDbModel, new() => DbCommandBuilder.Where<T>(sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : BaseDbModel, new() => DbCommandBuilder.Where(predicate);

        #endregion

        private static void CheckFoundMatch(DbModelDef modelDef, long foundMatch, object item, string lastUser, [CallerMemberName] string callerName = "")
        {
            ThrowIf.NullOrEmpty(callerName, nameof(callerName));

            if (foundMatch == 1)
            {
                return;
            }
            else if (foundMatch == 0)
            {
                //TODO: 这里返回0，一般是因为version不匹配，单也有可能是Id不存在，或者Deleted=1.
                //可以改造SqlHelper中的update语句为如下，进行一般改造，排除上述可能。
                //在原始的update语句，比如：update tb_userdirectorypermission set LastUser='TTTgdTTTEEST' where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0 and Version='0';
                //后面select found_rows(), count(1) as 'exits' from tb_userdirectorypermission where Id = uuid_to_bin('08da5b35-b123-2d4f-876c-6ee360db28c1') and Deleted = 0;
                //然后使用Reader读取，通过两个值进行判断。
                //如果found_rows=1，正常返回
                //如果found_rows=0, exists = 1, version冲突
                //如果found_rows=0, exists = 0, 已删除

                //不存在和Version冲突，统称为冲突，所以不用改，反正后续业务为了解决冲突也会重新select什么的，到时候可以判定是已经删掉了还是version冲突

                throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(item), $"{callerName} 发生冲突，或者不存在这样的ID. modelDef:{modelDef.FullName}, lastUser:{lastUser}");
            }
            else
            {
                throw DbExceptions.FoundTooMuch(modelDef.FullName, SerializeUtil.ToJson(item), $"{callerName} 发现太多！modelDef:{modelDef.FullName}, lastUser:{lastUser}");
            }
        }

        private static void CheckFoundMatches<T>(DbModelDef modelDef, IDataReader reader, IList<T> items, string lastUser, [CallerMemberName] string callerName = "")
        {
            ThrowIf.NullOrEmpty(callerName, nameof(callerName));

            int count = 0;

            while (reader.Read())
            {
                int matched = reader.GetInt32(0);

                if (matched == 1)
                {
                }
                else if (matched == 0)
                {
                    throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(items), $"{callerName}. 没有这样的ID，或者产生冲突！");
                }
                else
                {
                    throw DbExceptions.FoundTooMuch(modelDef.FullName, $"{callerName}: {SerializeUtil.ToJson(items)}, ModelDef:{modelDef.FullName}, lastUser:{lastUser}");
                }

                count++;
            }

            if (count != items.Count)
            {
                throw DbExceptions.ConcurrencyConflict(modelDef.FullName, SerializeUtil.ToJson(items), $"{callerName}: 数量不同.");
            }
        }

        private void ThrowIfExceedMaxBatchNumber<TObj>(IList<TObj> items, string lastUser, DbModelDef modelDef)
        {
            if (modelDef.DbSchema.MaxBatchNumber < items.Count)
            {
                throw DbExceptions.TooManyForBatch("BatchAdd超过批量操作的最大数目", items.Count, lastUser);
            }
        }

        private static void PrepareItem<T>(T item, string lastUser, ref string oldLastUser, ref long? oldTimestamp) where T : BaseDbModel, new()
        {
            long curTimestamp = TimeUtil.Timestamp;

            if (item is ITimestamp timestampModel)
            {
                oldTimestamp = timestampModel.Timestamp;
                timestampModel.Timestamp = curTimestamp;
            }

            oldLastUser = item.LastUser;
            item.LastUser = lastUser;
        }

        private static void RestoreItem<T>(T item, long? oldTimestamp, string oldLastUser) where T : BaseDbModel, new()
        {
            if (item is ITimestamp timestampModel)
            {
                timestampModel.Timestamp = oldTimestamp!.Value;
            }

            item.LastUser = oldLastUser;
        }

        private static void PrepareBatchItems<T>(IList<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : BaseDbModel, new()
        {
            long curTimestamp = TimeUtil.Timestamp;

            foreach (T item in items)
            {
                oldLastUsers.Add(item.LastUser);
                item.LastUser = lastUser;

                if (item is ITimestamp timestampModel)
                {
                    oldTimestamps.Add(timestampModel.Timestamp);

                    timestampModel.Timestamp = curTimestamp;
                }
            }
        }

        private static void RestoreBatchItems<T>(IList<T> items, IList<long> oldTimestamps, IList<string?> oldLastUsers, DbModelDef modelDef) where T : BaseDbModel, new()
        {
            for (int i = 0; i < items.Count; ++i)
            {
                T item = items[i];

                item.LastUser = oldLastUsers[i] ?? "";

                if (item is ITimestamp timestampModel)
                {
                    timestampModel.Timestamp = oldTimestamps[i];
                }
            }
        }
    }
}