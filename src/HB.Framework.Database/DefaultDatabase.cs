using HB.Framework.Database.SQL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using HB.Framework.Database.Entity;
using HB.Framework.Common;
using HB.Framework.Common.Entity;
using System.Threading.Tasks;
using HB.Framework.Database.Engine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private IDatabaseEngine _databaseEngine;
        private IDatabaseEntityDefFactory _entityDefFactory;
        private IDatabaseEntityMapper _modelMapper;
        private ISQLBuilder _sqlBuilder;
        private ILogger<DefaultDatabase> _logger;

        public IDatabaseEngine DatabaseEngine { get { return _databaseEngine; } }

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

        public IList<TSelect> Retrieve<TSelect, TFrom, TWhere>(Select<TSelect> selectCondition, From<TFrom> fromCondition, Where<TWhere> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.select(t => t.Id).select(t => t.Deleted).select(t => t.LastTime).select(t => t.LastUser).select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = new Where<TWhere>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false).And<TSelect>(ts=>ts.Deleted == false).And<TFrom>(tf=>tf.Deleted == false);

            #endregion

            IList<TSelect> result = null;
            IDataReader reader = null;
            DatabaseEntityDef selectDef = _entityDefFactory.Get<TSelect>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(selectCondition, fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, selectDef.DatabaseName, command, useMaster);
                result = _modelMapper.To<TSelect>(reader);
            }
            catch (Exception ex)
            {
                result = new List<TSelect>();
                _logger.LogError(ex.Message);

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

        public T Scalar<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
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

        public IList<T> Retrieve<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            #region Argument Adjusting

            if (selectCondition != null)
            {
                selectCondition.select(t => t.Id).select(t => t.Deleted).select(t => t.LastTime).select(t => t.LastUser).select(t => t.Version);
            }

            if (whereCondition == null)
            {
                whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false);

            #endregion

            IList<T> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.To<T>(reader);
            }
            catch (Exception ex)
            {
                result = new List<T>();
                _logger.LogError(ex.Message);
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

        public IList<T> Page<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.select(t => t.Id).select(t => t.Deleted).select(t => t.LastTime).select(t => t.LastUser).select(t => t.Version);
			}

			if (whereCondition == null)
			{
				whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

			whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster);
        }

        public long Count<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.select(t => t.Id).select(t => t.Deleted).select(t => t.LastTime).select(t => t.LastUser).select(t => t.Version);
			}

			if (whereCondition == null)
			{
				whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

            long count = -1;

			DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();
            try
            {
                IDbCommand command = _sqlBuilder.CreateCountCommand<T>(fromCondition, whereCondition);
                object countObj = _databaseEngine.ExecuteCommandScalar(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                count = Convert.ToInt32(countObj);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return count;
        }

        #endregion

        #region 单表查询, From, Where

        public T Scalar<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Retrieve<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public  IList<T> Page<T>(From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Page<T>(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public  long Count<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Count<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Where

        public IList<T> RetrieveAll<T>(DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Retrieve<T>(null, null, null, transContext, useMaster);
        }

        public T Scalar<T>(Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar<T>(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(Where<T> whereCondition, DbTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Retrieve<T>(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Page<T>(Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Page<T>(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public IList<T> Page<T>(long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Page<T>(null, null, null, pageNumber, perPageCount, transContext, useMaster);
        }

        public long Count<T>(Where<T> condition, DbTransactionContext transContext, bool useMaster) 
            where T : DatabaseEntity, new()
        {
            return Count<T>(null, null, condition, transContext, useMaster);
        }

        public long Count<T>(DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Count<T>(null, null, null, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Expression Where

        public T Scalar<T>(long id, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return Scalar<T>(t => t.Id == id && t.Deleted == false, transContext, useMaster);
        }

        public T Scalar<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return Scalar<T>(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Retrieve<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return Retrieve<T>(null, null, whereCondition, transContext, useMaster);
        }

        public IList<T> Page<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory).where(whereExpr);

            return Page<T>(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public long Count<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return Count<T>(null, null, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 双表查询

        public IList<Tuple<TSource, TTarget>> Retrieve<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);

            IList<Tuple<TSource, TTarget>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.Get<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget>(fromCondition, whereCondition);
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.To<TSource, TTarget>(reader);
            }
            catch (Exception ex)
            {
                result = new List<Tuple<TSource, TTarget>>();

                _logger.LogError(ex.Message);
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

        public IList<Tuple<TSource, TTarget>> Page<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Tuple<TSource, TTarget> Scalar<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
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

        public IList<Tuple<TSource, TTarget1, TTarget2>> Retrieve<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false)
                .And<TTarget1>(t => t.Deleted == false)
                .And<TTarget2>(t => t.Deleted == false);


            IList<Tuple<TSource, TTarget1, TTarget2>>  result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.Get<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition);

                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
                result = _modelMapper.To<TSource, TTarget1, TTarget2>(reader);
            }
            catch (Exception ex)
            {
                result = new List<Tuple<TSource, TTarget1, TTarget2>>();

                _logger.LogError(ex.Message);
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

        public IList<Tuple<TSource, TTarget1, TTarget2>> Page<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return Retrieve<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Tuple<TSource, TTarget1, TTarget2> Scalar<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
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
        public DatabaseResult Add<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }
  
            DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                reader = _databaseEngine.ExecuteCommandReader(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateAddCommand<T>(item, "default"), true);

                _modelMapper.To(reader, item);

                return DatabaseResult.Succeeded();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
        public DatabaseResult Delete<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            long id = item.Id;
            long version = item.Version;
            Where<T> condition = new Where<T>(_databaseEngine, _entityDefFactory).where(t => t.Id == id && t.Deleted == false && t.Version == version);

            try
            {
                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.GetDeleteCommand<T>(condition, "default"));

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }    

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        /// </summary>
        public DatabaseResult Update<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                return DatabaseResult.Fail("entity check failed.");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            Where<T> condition = new Where<T>(_databaseEngine, _entityDefFactory);

            long id = item.Id;
            long version = item.Version;

            condition.where(t => t.Id == id).And(t => t.Deleted == false);
            
            //版本控制
            condition.And(t => t.Version == version);

            try
            {
                long rows = _databaseEngine.ExecuteCommandNonQuery(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateUpdateCommand<T>(condition, item, "default"));

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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
        public DatabaseResult BatchAdd<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
        public DatabaseResult BatchUpdate<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
        public DatabaseResult BatchDelete<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
            DatabaseEntityDef entityDef = _entityDefFactory.Get<T>();

            return _databaseEngine.CreateTransaction(entityDef.DatabaseName, isolationLevel);
        }

        #endregion

        #region 条件构造

        public Select<T> Select<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewSelect<T>();
        }

        public From<T> From<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewFrom<T>();
        }

        public Where<T> Where<T>() where T : DatabaseEntity, new()
        {
            return _sqlBuilder.NewWhere<T>();
        }

        #endregion
    }
}
