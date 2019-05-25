using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using HB.Framework.Database.Entity;
using HB.Framework.Database.Engine;
using Microsoft.Extensions.Logging;
using HB.Framework.Database.Transaction;
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
    /// <typeparam name="T"></typeparam>
    public partial class DefaultDatabase : IDatabase
    {
        private readonly IDatabaseEngine _databaseEngine;
        private readonly IDatabaseEntityDefFactory _entityDefFactory;
        private IDatabaseEntityMapper _modelMapper;
        private ISQLBuilder _sqlBuilder;
        private ILogger<DefaultDatabase> _logger;

        //public IDatabaseEngine DatabaseEngine { get { return _databaseEngine; } }

        public DefaultDatabase(IDatabaseEngine databaseEngine, IDatabaseEntityDefFactory modelDefFactory, IDatabaseEntityMapper modelMapper, ISQLBuilder sqlBuilder, ILogger<DefaultDatabase> logger)
        {
            _databaseEngine = databaseEngine;
            _entityDefFactory = modelDefFactory;
            _modelMapper = modelMapper;
            _sqlBuilder = sqlBuilder;
            _logger = logger;
        }

        #region Private methods

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

        public IList<TSelect> Retrieve<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition, DatabaseTransactionContext transContext = null, bool useMaster = false)
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
                whereCondition = new WhereExpression<TWhere>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false).And<TSelect>(ts=>ts.Deleted == false).And<TFrom>(tf=>tf.Deleted == false);

            #endregion

            IList<TSelect> result = null;
            IDataReader reader = null;
            DatabaseEntityDef selectDef = _entityDefFactory.GetDef<TSelect>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand(selectCondition, fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, selectDef.DatabaseName, command, useMaster);
                result = _modelMapper.ToList<TSelect>(reader);
            }
            catch (DbException ex)
            {
                result = new List<TSelect>();
                _logger.LogCritical(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public T Scalar<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            IList<T> result = Retrieve<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                _logger.LogCritical(0, "retrieve result not one, but many." + typeof(T).FullName, null);
                return null;
            }

            return result[0];
        }

        public IList<T> Retrieve<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false);

            #endregion

            IList<T> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.ToList<T>(reader);
            }
            catch (DbException ex)
            {
                result = new List<T>();
                _logger.LogCritical(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<T> Page<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
			}

			if (whereCondition == null)
			{
				whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

			whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster);
        }

        public long Count<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
			}

			if (whereCondition == null)
			{
				whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

            long count = -1;

			DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();
            try
            {
                IDbCommand command = _sqlBuilder.CreateCountCommand(fromCondition, whereCondition);
                object countObj = _databaseEngine.ExecuteCommandScalar(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                count = Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (DbException ex)
            {
                _logger.LogCritical(ex.Message);
            }

            return count;
        }

        #endregion

        #region 单表查询, From, Where

        public T Scalar<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Retrieve(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public  IList<T> Page<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Page(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public  long Count<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Count(null, fromCondition, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Where

        public IList<T> RetrieveAll<T>(DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Retrieve<T>(null, null, null, transContext, useMaster);
        }

        public T Scalar<T>(WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Retrieve(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Page<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Page(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public IList<T> Page<T>(long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Page<T>(null, null, null, pageNumber, perPageCount, transContext, useMaster);
        }

        public long Count<T>(WhereExpression<T> condition, DatabaseTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Count(null, null, condition, transContext, useMaster);
        }

        public long Count<T>(DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Count<T>(null, null, null, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Expression Where

        public T Scalar<T>(long id, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar<T>(t => t.Id == id && t.Deleted == false, transContext, useMaster);
        }

        public T Scalar<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return Scalar(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return Retrieve(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory).Where(whereExpr);

            return Page(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public long Count<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return Count(null, null, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 双表查询

        public IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            switch(fromCondition.JoinType)
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
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.ToList<TSource, TTarget>(reader);
            }
            catch (DbException ex)
            {
                result = new List<Tuple<TSource, TTarget>>();

                _logger.LogCritical(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return result;
        }

        public IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Tuple<TSource, TTarget> Scalar<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            IList<Tuple<TSource, TTarget>> result = Retrieve<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                _logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                return null;
            }

            return result[0];
        }

        #endregion

        #region 三表查询

        public IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
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


            IList<Tuple<TSource, TTarget1, TTarget2>>  result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.ToList<TSource, TTarget1, TTarget2>(reader);
            }
            catch (DbException ex)
            {
                result = new List<Tuple<TSource, TTarget1, TTarget2>>();

                _logger.LogCritical(ex.Message);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }


            return result;
        }

        public IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            IList<Tuple<TSource, TTarget1, TTarget2>> result = Retrieve<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster);

            if (result == null || result.Count == 0)
            {
                return null;
            }

            if (result.Count > 1)
            {
                _logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                return null;
            }

            return result[0];
        }

        #endregion

        #region 单体更改(Write)

        /// <summary>
        /// 增加,并且item被重新赋值
        /// </summary>
        public DatabaseResult Add<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateAddCommand(item, "default"), true);

                _modelMapper.ToObject(reader, item);

                return DatabaseResult.Succeeded();
            }
            catch (DbException ex)
            {
                _logger.LogCritical(ex.Message);
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
        public DatabaseResult Delete<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
            WhereExpression<T> condition = new WhereExpression<T>(_databaseEngine, _entityDefFactory).Where(t => t.Id == id && t.Deleted == false && t.Version == version);

            try
            {
                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.GetDeleteCommand(condition, "default"));

                if (rows == 1)
                {
                    return DatabaseResult.Succeeded();
                }
                else if(rows == 0)
                {
                    return DatabaseResult.NotFound();
                }

                throw new Exception("Multiple Rows Affected instead of one. Something go wrong.");
            }
            catch (DbException ex)
            {
                _logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }    

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        /// </summary>
        public DatabaseResult Update<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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

            WhereExpression<T> condition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);

            long id = item.Id;
            long version = item.Version;

            condition.Where(t => t.Id == id).And(t => t.Deleted == false);
            
            //版本控制
            condition.And(t => t.Version == version);

            try
            {
                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateUpdateCommand(condition, item, "default"));

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
                _logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }

        #endregion

        #region 批量更改(Write), 无版本控制


        /// <summary>
        /// 批量添加,返回新产生的ID列表
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DatabaseResult BatchAdd<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
        {
            throw new NotImplementedException();
            //if (!CheckEntities<T>(items))
            //{
            //    return DatabaseResult.Fail("entities not valid.");
            //}

            //DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            //if (!entityDef.DatabaseWriteable)
            //{
            //    return DatabaseResult.NotWriteable;
            //}

            //try
            //{
            //    DatabaseResult result = DatabaseResult.Succeeded;

            //    string sql = _sqlBuilder.GetBatchAddStatement<T>(items, lastUser);

            //    using (IDataReader reader = _databaseEngine.ExecuteSqlReader(transContext?.Transaction, entityDef.DatabaseName, sql, true))
            //    {
            //        while (reader.Read())
            //        {
            //            result.AddId(reader.GetInt32(0));
            //        }
            //    }

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);

            //    return DatabaseResult.Fail(ex);
            //}
        }
        
        /// <summary>
        /// 批量更改,无版本控制
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public DatabaseResult BatchUpdate<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
        {
            throw new NotImplementedException();
            //if (!checkEntities<T>(items))
            //{
            //    return DatabaseResult.Fail("entities not valid.");
            //}

            //DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            //if (!entityDef.DatabaseWriteable)
            //{
            //    return DatabaseResult.NotWriteable;
            //}

            //try
            //{
            //    DatabaseResult result = DatabaseResult.Succeeded;
            //    string sql = _sqlBuilder.GetBatchUpdateStatement<T>(items, lastUser);

            //    using (IDataReader reader = _databaseEngine.ExecuteSqlReader(trans, entityDef.DatabaseName, sql, true))
            //    {
            //        while (reader.Read())
            //        {
            //            result.AddId(reader.GetInt32(0));
            //        }
            //    }

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);

            //    return DatabaseResult.Fail(ex);
            //}
        }
        
        /// <summary>
        /// 批量删除,返回影响行数列表,无版本控制
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public DatabaseResult BatchDelete<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
        {
            throw new NotImplementedException();
            //if (!checkEntities<T>(items))
            //{
            //    return DatabaseResult.Fail("Entities not valid");
            //}

            //DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            //if (!entityDef.DatabaseWriteable)
            //{
            //    return DatabaseResult.NotWriteable;
            //}

            //try
            //{
            //    DatabaseResult result = DatabaseResult.Succeeded;
            //    string sql = _sqlBuilder.GetBatchDeleteStatement<T>(items, lastUser);

            //    using (IDataReader reader = _databaseEngine.ExecuteSqlReader(trans, entityDef.DatabaseName, sql, true))
            //    {
            //        while (reader.Read())
            //        {
            //            result.AddId(reader.GetInt32(0));
            //        }
            //    }

            //    return DatabaseResult.Succeeded;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);

            //    return DatabaseResult.Fail(ex);
            //}
        }

        #endregion

        #region 事务

        public IDbTransaction CreateTransaction<T>(IsolationLevel isolationLevel) where T : DatabaseEntity
        {
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            return _databaseEngine.CreateTransaction(entityDef.DatabaseName, isolationLevel);
        }

        #endregion

        #region 条件构造

        public SelectExpression<T> NewSelect<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewSelect<T>();
        }

        public FromExpression<T> NewFrom<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewFrom<T>();
        }

        public WhereExpression<T> NewWhere<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewWhere<T>();
        }

        #endregion
    }
}
