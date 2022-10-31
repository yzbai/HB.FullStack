
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.Extensions;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

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
    public sealed partial class DefaultDatabase : IDatabase
    {
        private readonly ILogger _logger;

        private ITransaction Transaction { get; }

        private IDbManager DbManager { get; }

        public IDbModelDefFactory ModelDefFactory { get; }

        public IDbCommandBuilder DbCommandBuilder { get; }
       

        public DefaultDatabase(
            ILogger<DefaultDatabase> logger,
            IDbManager dbManager,
            IDbModelDefFactory modelDefFactory,
            IDbCommandBuilder commandBuilder,
            ITransaction transaction)
        {
            _logger = logger;
            Transaction = transaction;

            ModelDefFactory = modelDefFactory;
            DbManager = dbManager;
            DbCommandBuilder = commandBuilder;
        }

        public Task<bool> InitializeAsync(DbSchema dbSchema, string? connectionString, IList<string>? slaveConnectionStrings, IEnumerable<Migration>? migrations) 
            => DbManager.InitializeAsync(dbSchema, connectionString, slaveConnectionStrings, migrations);

        #region 条件构造

        public FromExpression<T> From<T>() where T : DbModel, new() => DbCommandBuilder.From<T>();

        public WhereExpression<T> Where<T>() where T : DbModel, new() => DbCommandBuilder.Where<T>();

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : DbModel, new() => DbCommandBuilder.Where<T>(sqlFilter, filterParams);

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : DbModel, new() => DbCommandBuilder.Where(predicate);

        #endregion

        private static void PrepareBatchItems<T>(IEnumerable<T> items, string lastUser, List<long> oldTimestamps, List<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            long timestamp = TimeUtil.Timestamp;

            foreach (var item in items)
            {
                if (item is TimestampDbModel tsItem)
                {
                    oldTimestamps.Add(tsItem.Timestamp);
                    oldLastUsers.Add(tsItem.LastUser);

                    tsItem.Timestamp = timestamp;
                    tsItem.LastUser = lastUser;
                }
            }
        }

        private static void RestoreBatchItems<T>(IEnumerable<T> items, IList<long> oldTimestamps, IList<string?> oldLastUsers, DbModelDef modelDef) where T : DbModel, new()
        {
            if (!modelDef.IsTimestampDBModel)
            {
                return;
            }

            for (int i = 0; i < items.Count(); ++i)
            {
                if (items.ElementAt(i) is TimestampDbModel tsItem)
                {
                    tsItem.Timestamp = oldTimestamps[i];
                    tsItem.LastUser = oldLastUsers[i] ?? "";
                }
            }
        }

        private static void ThrowIfNotWriteable(DbModelDef modelDef)
        {
            if (!modelDef.DbWriteable)
            {
                throw DatabaseExceptions.NotWriteable(type: modelDef.ModelFullName, database: modelDef.DbSchema);
            }
        }

        private void TruncateLastUser(ref string lastUser)
        {
            if (lastUser.Length > DefaultLengthConventions.MAX_LAST_USER_LENGTH)
            {
                _logger.LogWarning("LastUser 截断. {LastUser}", lastUser);

                lastUser = lastUser[..DefaultLengthConventions.MAX_LAST_USER_LENGTH];
            }
        }
        
    }
}