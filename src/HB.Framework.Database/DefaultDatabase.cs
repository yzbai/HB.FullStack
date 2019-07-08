using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;
using System.Data.Common;

namespace HB.Framework.Database
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
    internal partial class DefaultDatabase : IDatabase
    {
        private static object _lockerObj = new object();

        private bool _initialized = false;

        private readonly IDatabaseSettings _databaseSettings;
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        private IDatabaseEntityMapper _modelMapper;
        private ISQLBuilder _sqlBuilder;
        //private ILogger<DefaultDatabase> _logger;

        //public IDatabaseEngine DatabaseEngine { get { return _databaseEngine; } }

        public DefaultDatabase(IDatabaseSettings databaseSettings, IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory modelDefFactory, IDatabaseEntityMapper modelMapper, ISQLBuilder sqlBuilder/*, ILogger<DefaultDatabase> logger*/)
        {
            if (databaseSettings.Version < 0)
            {
                throw new ArgumentException("Database Version should greater than 0");
            }

            _databaseSettings = databaseSettings;
            _databaseEngine = databaseEngine;
            _entityDefFactory = modelDefFactory;
            _modelMapper = modelMapper;
            _sqlBuilder = sqlBuilder;
            //_logger = logger;
        }

        #region Initialize

        public void Initialize(IList<Migration> migrations = null)
        {
            if (!_initialized)
            {
                lock (_lockerObj)
                {
                    if (!_initialized)
                    {
                        _initialized = true;

                        AutoCreateTablesIfBrandNew();

                        Migarate(migrations);
                    }
                }
            }
        }

        private void AutoCreateTablesIfBrandNew()
        {
            if (!_databaseSettings.AutomaticCreateTable)
            {
                return;
            }

            _databaseEngine.GetDatabaseNames().ForEach(databaseName => {

                TransactionContext transactionContext = BeginTransaction(databaseName, IsolationLevel.Serializable);

                try
                {
                    SystemInfo sys = _databaseEngine.GetSystemInfo(databaseName, transactionContext.Transaction);
                    //表明是新数据库
                    if (sys.Version == 0)
                    {
                        if (_databaseSettings.Version != 1)
                        {
                            Rollback(transactionContext);
                            throw new DatabaseException($"Database:{databaseName} does not exists, database Version must be 1");
                        }

                        CreateTablesByDatabase(databaseName, transactionContext);

                        _databaseEngine.UpdateSystemVersion(databaseName, 1, transactionContext.Transaction);
                    }

                    Commit(transactionContext);
                }
                catch (Exception ex)
                {
                    Rollback(transactionContext);
                    throw new DatabaseException($"Auto Create Table Failed, Database:{databaseName}, Reason:{ex.Message}", ex);
                }

            });
        }

        private int CreateTable(DatabaseEntityDef def, TransactionContext transContext)
        {
            string sql = GetTableCreateStatement(def.EntityType, false);

            return _databaseEngine.ExecuteSqlNonQuery(transContext.Transaction, def.DatabaseName, sql);
        }

        private void CreateTablesByDatabase(string databaseName, TransactionContext transactionContext)
        {
            _entityDefFactory
                .GetAllDefsByDatabase(databaseName)
                .ForEach(def => CreateTable(def, transactionContext));
        }

        private void Migarate(IList<Migration> migrations)
        {
            if (migrations != null && migrations.Any(m => m.NewVersion <= m.OldVersion))
            {
                throw new DatabaseException($"oldVersion should always lower than newVersions in Database Migrations");
            }

            _databaseEngine.GetDatabaseNames().ForEach(databaseName => {

                TransactionContext transactionContext = BeginTransaction(databaseName, IsolationLevel.Serializable);

                try
                {
                    SystemInfo sys = _databaseEngine.GetSystemInfo(databaseName, transactionContext.Transaction);

                    if (sys.Version < _databaseSettings.Version)
                    {
                        if (migrations == null)
                        {
                            throw new DatabaseException($"Lack Migrations for {sys.DatabaseName}");
                        }

                        IOrderedEnumerable<Migration> curOrderedMigrations = migrations
                            .Where(m => m.TargetSchema.Equals(sys.DatabaseName, GlobalSettings.ComparisonIgnoreCase))
                            .OrderBy(m => m.OldVersion);

                        if (curOrderedMigrations == null)
                        {
                            throw new DatabaseException($"Lack Migrations for {sys.DatabaseName}");
                        }

                        if (!CheckMigration(sys.Version, _databaseSettings.Version, curOrderedMigrations))
                        {
                            throw new DatabaseException($"Can not perform Migration on ${sys.DatabaseName}, because the migrations provided is not sufficient.");
                        }

                        curOrderedMigrations.ForEach(migration => _databaseEngine.ExecuteSqlNonQuery(transactionContext.Transaction, databaseName, migration.SqlStatement));

                        _databaseEngine.UpdateSystemVersion(sys.DatabaseName, _databaseSettings.Version, transactionContext.Transaction);
                    }

                    Commit(transactionContext);
                }
                catch (Exception ex)
                {
                    Rollback(transactionContext);
                    throw new DatabaseException($"Migration Failed at Database:{databaseName}", ex);
                }
            });
        }

        private bool CheckMigration(int startVersion, int endVersion, IOrderedEnumerable<Migration> curOrderedMigrations)
        {
            int curVersion = curOrderedMigrations.ElementAt(0).OldVersion;

            if (curVersion != startVersion) { return false; }

            foreach (Migration migration in curOrderedMigrations)
            {
                if (curVersion != migration.OldVersion)
                {
                    return false;
                }

                curVersion = migration.NewVersion;
            }

            return curVersion == endVersion;
        }

        #endregion

        #region Private methods

        //private static void bindCommandTransaction(TransactionContext transContext, IDbCommand command)
        //{
        //    if (transContext != null)
        //    {
        //        command.Transaction = transContext.Transaction;
        //    }
        //}

        private static bool CheckEntities<T>(IEnumerable<T> items) where T : DatabaseEntity, new()
        {
            if (items == null || items.Count() == 0)
            {
                return true;
            }

            return items.All(t => t.IsValid());
        }

        #endregion

        #region 单表查询, Select, From, Where

        public IList<TSelect> Retrieve<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition, TransactionContext transContext = null)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = Where<TWhere>();
            }

            whereCondition.And(t => t.Deleted == false).And<TSelect>(ts => ts.Deleted == false).And<TFrom>(tf => tf.Deleted == false);

            #endregion

            IList<TSelect> result = null;
            IDataReader reader = null;
            DatabaseEntityDef selectDef = _entityDefFactory.GetDef<TSelect>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand(selectCondition, fromCondition, whereCondition);
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, selectDef.DatabaseName, command, transContext != null);
                result = _modelMapper.ToList<TSelect>(reader);
            }
            //catch (DbException ex)
            //{
            //    throw ex;
            //}
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public T Scalar<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            IList<T> result = Retrieve<T>(selectCondition, fromCondition, whereCondition, transContext);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                //_logger.LogCritical(0, "retrieve result not one, but many." + typeof(T).FullName, null);

                throw new DatabaseException($"Scalar retrieve return more than one result. Select:{selectCondition.ToString()}, From:{fromCondition.ToString()}, Where:{whereCondition.ToString()}");
            }

            return result[0];
        }

        public IList<T> Retrieve<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            whereCondition.And(t => t.Deleted == false);

            #endregion

            IList<T> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null);
                result = _modelMapper.ToList<T>(reader);
            }
            //catch (DbException ex)
            //{
            //    result = new List<T>();
            //    _logger.LogCritical(ex.Message);
            //}
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<T> Page<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            whereCondition.And(t => t.Deleted == false);

            #endregion

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<T>(selectCondition, fromCondition, whereCondition, transContext);
        }

        public long Count<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = Where<T>();
            }

            whereCondition.And(t => t.Deleted == false);

            #endregion

            long count = -1;

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();
            try
            {
                IDbCommand command = _sqlBuilder.CreateCountCommand(fromCondition, whereCondition);
                object countObj = _databaseEngine.ExecuteCommandScalar(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null);
                count = Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (DbException ex)
            {
                throw ex;// _logger.LogCritical(ex.Message);
            }

            return count;
        }

        #endregion

        #region 单表查询, From, Where

        public T Scalar<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Scalar(null, fromCondition, whereCondition, transContext);
        }

        public IList<T> Retrieve<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Retrieve(null, fromCondition, whereCondition, transContext);
        }

        public IList<T> Page<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Page(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext);
        }

        public long Count<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Count(null, fromCondition, whereCondition, transContext);
        }

        #endregion

        #region 单表查询, Where

        public IList<T> RetrieveAll<T>(TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Retrieve<T>(null, null, null, transContext);
        }

        public T Scalar<T>(WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Scalar(null, null, whereCondition, transContext);
        }

        public IList<T> Retrieve<T>(WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Retrieve(null, null, whereCondition, transContext);
        }

        public IList<T> Page<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Page(null, null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public IList<T> Page<T>(long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Page<T>(null, null, null, pageNumber, perPageCount, transContext);
        }

        public long Count<T>(WhereExpression<T> condition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Count(null, null, condition, transContext);
        }

        public long Count<T>(TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Count<T>(null, null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public T Scalar<T>(long id, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return Scalar<T>(t => t.Id == id && t.Deleted == false, transContext);
        }

        public T Scalar<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return Scalar(null, null, whereCondition, transContext);
        }

        public IList<T> Retrieve<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return Retrieve(null, null, whereCondition, transContext);
        }

        public IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where<T>().Where(whereExpr);

            return Page(null, null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public long Count<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = Where<T>();
            whereCondition.Where(whereExpr);

            return Count(null, null, whereCondition, transContext);
        }

        #endregion

        #region 双表查询

        public IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And(t => t.Deleted == false);
                    break;
                case SqlJoinType.RIGHT:
                    whereCondition.And<TTarget>(t => t.Deleted == false);
                    break;
                case SqlJoinType.INNER:
                    whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
                case SqlJoinType.FULL:
                    break;
                case SqlJoinType.CROSS:
                    whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
            }

            IList<Tuple<TSource, TTarget>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget>(fromCondition, whereCondition);
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null);
                result = _modelMapper.ToList<TSource, TTarget>(reader);
            }
            //catch (DbException ex)
            //{
            //    result = new List<Tuple<TSource, TTarget>>();

            //    _logger.LogCritical(ex.Message);
            //}
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget>(fromCondition, whereCondition, transContext);
        }

        public Tuple<TSource, TTarget> Scalar<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            IList<Tuple<TSource, TTarget>> result = Retrieve<TSource, TTarget>(fromCondition, whereCondition, transContext);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                throw new DatabaseException($"Scalar retrieve return more than one result. From:{fromCondition.ToString()}, Where:{whereCondition.ToString()}");
                //_logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                //return null;
            }

            return result[0];
        }

        #endregion

        #region 三表查询

        public IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And(t => t.Deleted == false);
                    break;
                case SqlJoinType.RIGHT:
                    whereCondition.And<TTarget2>(t => t.Deleted == false);
                    break;
                case SqlJoinType.INNER:
                    whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
                case SqlJoinType.FULL:
                    break;
                case SqlJoinType.CROSS:
                    whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
            }


            IList<Tuple<TSource, TTarget1, TTarget2>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition);
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null);
                result = _modelMapper.ToList<TSource, TTarget1, TTarget2>(reader);
            }
            //catch (DbException ex)
            //{
            //    result = new List<Tuple<TSource, TTarget1, TTarget2>>();

            //    _logger.LogCritical(ex.Message);
            //}
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }


            return result;
        }

        public IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = Where<TSource>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext);
        }

        public Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            IList<Tuple<TSource, TTarget1, TTarget2>> result = Retrieve<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                throw new DatabaseException($"Scalar retrieve return more than one result. From:{fromCondition.ToString()}, Where:{whereCondition.ToString()}");
                //_logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                //return null;
            }

            return result[0];
        }

        #endregion

        #region 单体更改(Write)

        /// <summary>
        /// 增加,并且item被重新赋值
        /// </summary>
        public DatabaseResult Add<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                IDbCommand dbCommand = _sqlBuilder.CreateAddCommand(item, "default");

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, dbCommand, true);

                _modelMapper.ToObject(reader, item);

                return DatabaseResult.Succeeded();
            }
            catch (DbException ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 删除, Version控制
        /// </summary>
        public DatabaseResult Delete<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            long id = item.Id;
            long version = item.Version;
            WhereExpression<T> condition = Where<T>().Where(t => t.Id == id && t.Deleted == false && t.Version == version);

            try
            {
                IDbCommand dbCommand = _sqlBuilder.GetDeleteCommand(condition, "default");

                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, dbCommand);

                if (rows == 1)
                {
                    return DatabaseResult.Succeeded();
                }
                else if (rows == 0)
                {
                    return DatabaseResult.NotFound();
                }

                throw new Exception("Multiple Rows Affected instead of one. Something go wrong.");
            }
            catch (DbException ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        /// </summary>
        public DatabaseResult Update<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            WhereExpression<T> condition = Where<T>();

            long id = item.Id;
            long version = item.Version;

            condition.Where(t => t.Id == id).And(t => t.Deleted == false);

            //版本控制
            condition.And(t => t.Version == version);

            try
            {
                IDbCommand dbCommand = _sqlBuilder.CreateUpdateCommand(condition, item, "default");

                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, dbCommand);

                if (rows == 1)
                {
                    item.Version++;
                    return DatabaseResult.Succeeded();
                }
                else if (rows == 0)
                {
                    return DatabaseResult.NotFound();
                }

                throw new Exception("Multiple Rows Affected instead of one. Something go wrong.");
            }
            catch (DbException ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }

        #endregion

        #region 批量更改(Write)


        /// <summary>
        /// 批量添加,返回新产生的ID列表
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DatabaseResult BatchAdd<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (transContext == null)
            {
                return DatabaseResult.Fail(new ArgumentNullException(nameof(transContext)));
            }

            if (!CheckEntities<T>(items))
            {
                return DatabaseResult.Fail("entities not valid.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                DatabaseResult result = DatabaseResult.Succeeded();

                IDbCommand dbCommand = _sqlBuilder.CreateBatchAddStatement(items, lastUser);

                reader = _databaseEngine.ExecuteCommandReader(
                    transContext.Transaction,
                    entityDef.DatabaseName,
                    dbCommand,
                    true);

                while (reader.Read())
                {
                    int newId = reader.GetInt32(0);

                    if (newId <= 0)
                    {
                        throw new DatabaseException("BatchAdd wrong new id return.");
                    }

                    result.AddId(newId);
                }

                if (result.Ids.Count != items.Count())
                    throw new DatabaseException("BatchAdd wrong new id number return.");

                return result;
            }
            catch (Exception ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        /// <summary>
        /// 批量更改
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DatabaseResult BatchUpdate<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (transContext == null)
            {
                return DatabaseResult.Fail(new ArgumentNullException(nameof(transContext)));
            }

            if (!CheckEntities<T>(items))
            {
                return DatabaseResult.Fail("entities not valid.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                IDbCommand dbCommand = _sqlBuilder.CreateBatchUpdateStatement(items, lastUser);

                reader = _databaseEngine.ExecuteCommandReader(
                    transContext.Transaction,
                    entityDef.DatabaseName,
                    dbCommand,
                    true);

                int count = 0;

                while (reader.Read())
                {
                    int matched = reader.GetInt32(0);

                    if (matched != 1)
                    {
                        throw new DatabaseException("BatchUpdate wrong, no match the {" + count + "}th data item. ");
                    }

                    count++;
                }

                if (count != items.Count())
                    throw new DatabaseException("BatchUpdate wrong number return.");

                return DatabaseResult.Succeeded();
            }
            catch (Exception ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        public DatabaseResult BatchDelete<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (transContext == null)
            {
                return DatabaseResult.Fail(new ArgumentNullException(nameof(transContext)));
            }

            if (!CheckEntities<T>(items))
            {
                return DatabaseResult.Fail("Entities not valid");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                IDbCommand dbCommand = _sqlBuilder.CreateBatchDeleteStatement(items, lastUser);

                reader = _databaseEngine.ExecuteCommandReader(
                    transContext.Transaction,
                    entityDef.DatabaseName,
                    dbCommand,
                    true);

                int count = 0;

                while (reader.Read())
                {
                    int affected = reader.GetInt32(0);

                    if (affected != 1)
                    {
                        throw new DatabaseException("BatchDelete wrong, no affected the {" + count + "}th data item. ");
                    }

                    count++;
                }

                if (count != items.Count())
                    throw new DatabaseException("BatchDelete wrong number return.");

                return DatabaseResult.Succeeded();
            }
            catch (Exception ex)
            {
                //_logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }
        }

        #endregion

        #region 条件构造

        public SelectExpression<T> Select<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewSelect<T>();
        }

        public FromExpression<T> From<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewFrom<T>();
        }

        public WhereExpression<T> Where<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewWhere<T>();
        }

        #endregion

        #region 表创建SQL

        public string GetTableCreateStatement(Type type, bool addDropStatement)
        {
            return _sqlBuilder.GetTableCreateStatement(type, addDropStatement);
        }

        #endregion

        #region 事务

        public TransactionContext BeginTransaction(string databaseName, IsolationLevel isolationLevel)
        {
            IDbTransaction dbTransaction = _databaseEngine.BeginTransaction(databaseName, isolationLevel);

            return new TransactionContext() {
                Transaction = dbTransaction,
                Status = TransactionStatus.InTransaction
            };
        }

        public TransactionContext BeginTransaction<T>(IsolationLevel isolationLevel) where T : DatabaseEntity
        {
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return BeginTransaction(entityDef.DatabaseName, isolationLevel);
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit(TransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Status == TransactionStatus.Commited)
            {
                return;
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException("use a already finished transactioncontenxt");
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;
                _databaseEngine.Commit(context.Transaction);
                //context.Transaction.Commit();
                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = TransactionStatus.Commited;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback(TransactionContext context)
        {
            if (context == null || context.Transaction == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Status == TransactionStatus.Rollbacked)
            {
                return;
            }

            if (context.Status != TransactionStatus.InTransaction)
            {
                throw new DatabaseException("use a already finished transactioncontenxt");
            }

            try
            {
                IDbConnection conn = context.Transaction.Connection;

                _databaseEngine.Rollback(context.Transaction);

                //context.Transaction.Rollback();
                context.Transaction.Dispose();

                if (conn != null && conn.State != ConnectionState.Closed)
                {
                    conn.Dispose();
                }

                context.Status = TransactionStatus.Rollbacked;
            }
            catch
            {
                context.Status = TransactionStatus.Failed;
                throw;
            }
        }

        #endregion
    }
}
