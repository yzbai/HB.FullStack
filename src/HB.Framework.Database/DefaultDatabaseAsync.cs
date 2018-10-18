using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using Microsoft.Extensions.Logging;

namespace HB.Framework.Database
{
    public partial class DefaultDatabase
    {
        #region 单表查询, Select, From, Where

        public Task<T> ScalarAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster)
                .ContinueWith(t=> {
                    IList<T> lst = t.Result;

                    if (lst == null || lst.Count == 0)
                    {
                        return default(T);
                    }

                    if (lst.Count > 1)
                    {
                        _logger.LogCritical(0, "retrieve result not one, but many." + typeof(T).FullName, null);
                        return default(T);
                    }

                    return lst[0];
                }, TaskScheduler.Default);
        }

        public async Task<IList<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(Select<TSelect> selectCondition, From<TFrom> fromCondition, Where<TWhere> whereCondition, DbTransactionContext transContext = null, bool useMaster = false)
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

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName, command, useMaster);
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

        public async Task<IList<T>> RetrieveAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
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
            DatabaseEntityDef modelDef = _entityDefFactory.Get<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName, command, useMaster);
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

        public Task<IList<T>> PageAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
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

            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster);
        }

        public async Task<long> CountAsync<T>(Select<T> selectCondition, From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
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
                object countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
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

        public Task<T> ScalarAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(From<T> fromCondition, Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(From<T> fromCondition, Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, fromCondition, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Where

        public Task<IList<T>> RetrieveAllAsync<T>(DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, null, null, transContext, useMaster);
        }

        public Task<T> ScalarAsync<T>(Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync<T>(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(Where<T> whereCondition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(Where<T> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, null, null, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(Where<T> condition, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, null, condition, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, null, null, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T> ScalarAsync<T>(long id, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync<T>(t => t.Id == id && t.Deleted == false, transContext, useMaster);
        }

        //public Task<T> RetrieveScalaAsyncr<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster = false) where T : DatabaseEntity, new();
        public Task<T> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster = false) where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return ScalarAsync<T>(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return RetrieveAsync<T>(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory).where(whereExpr);

            return PageAsync<T>(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, DbTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            Where<T> whereCondition = new Where<T>(_databaseEngine, _entityDefFactory);
            whereCondition.where(whereExpr);

            return CountAsync<T>(null, null, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 双表查询

        public async Task<IList<Tuple<TSource, TTarget>>> RetrieveAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
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
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
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

        public Task<IList<Tuple<TSource, TTarget>>> PageAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<Tuple<TSource, TTarget>> ScalarAsync<TSource, TTarget>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster)
                .ContinueWith(t =>
                {
                    IList<Tuple<TSource, TTarget>> lst = t.Result;

                    if (lst == null || lst.Count == 0)
                    {
                        return null;
                    }

                    if (lst.Count > 1)
                    {
                        _logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                        return null;
                    }

                    return lst[0];

                }, TaskScheduler.Default);
        }

        #endregion

        #region 三表查询

        public async Task<IList<Tuple<TSource, TTarget1, TTarget2>>> RetrieveAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
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


            IList<Tuple<TSource, TTarget1, TTarget2>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.Get<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster);
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

        public Task<IList<Tuple<TSource, TTarget1, TTarget2>>> PageAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, long pageNumber, long perPageCount, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new Where<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<Tuple<TSource, TTarget1, TTarget2>> ScalarAsync<TSource, TTarget1, TTarget2>(From<TSource> fromCondition, Where<TSource> whereCondition, DbTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster)
                .ContinueWith(t=> {
                    IList<Tuple<TSource, TTarget1, TTarget2>> lst = t.Result;

                    if (lst == null || lst.Count == 0)
                    {
                        return null;
                    }

                    if (lst.Count > 1)
                    {
                        _logger.LogCritical(0, "retrieve result not one, but many." + typeof(TSource).FullName, null);
                        return null;
                    }

                    return lst[0];
                }, TaskScheduler.Default);
        }

        #endregion

        #region 单体更改(Write)

        /// <summary>
        /// 增加,并且item被重新赋值
        /// </summary>
        public async Task<DatabaseResult> AddAsync<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateAddCommand<T>(item, "default"), true);

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
        public async Task<DatabaseResult> DeleteAsync<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.GetDeleteCommand<T>(condition, "default"));

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
        public async Task<DatabaseResult> UpdateAsync<T>(T item, DbTransactionContext transContext) where T : DatabaseEntity, new()
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
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateUpdateCommand<T>(condition, item, "default"));

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

        //TODO: 加锁

        /// <summary>
        /// 批量添加,返回新产生的ID列表
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Task<DatabaseResult> BatchAddAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
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

            //IDataReader reader = null;
            //DatabaseResult result = DatabaseResult.Succeeded;

            //try
            //{
            //    string sql = _sqlBuilder.GetBatchAddStatement<T>(items, lastUser);

            //    reader = await _databaseEngine.ExecuteSqlReaderAsync(trans, entityDef.DatabaseName, sql, true);

            //    while (reader.Read())
            //    {
            //        result.AddId(reader.GetInt32(0));
            //    }

            //    return result;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex.Message);

            //    return DatabaseResult.Fail(ex);
            //}
            //finally
            //{
            //    if (reader != null)
            //    {
            //        reader.Dispose();
            //    }
            //}
        }

        /// <summary>
        /// 批量更改,返回影响行数列表
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Task<DatabaseResult> BatchUpdateAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
        {
            throw new NotImplementedException();
            //updatedIds = new List<long>();

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
            //    string sql = _sqlBuilder.GetBatchUpdateStatement<T>(items, lastUser);
            //    using (IDataReader reader = _databaseEngine.ExecuteSqlReader(trans, entityDef.DatabaseName, sql, true))
            //    {
            //        while (reader.Read())
            //        {
            //            updatedIds.Add(reader.GetInt32(0));
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

        /// <summary>
        /// 批量删除,返回影响行数列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public Task<DatabaseResult> BatchDeleteAsync<T>(IList<T> items, string lastUser, DbTransactionContext transContext) where T : DatabaseEntity, new()
        {
            throw new NotImplementedException();
            //deletedIds = new List<long>();

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
            //    string sql = _sqlBuilder.GetBatchDeleteStatement<T>(items, lastUser);

            //    using (IDataReader reader = _databaseEngine.ExecuteSqlReader(trans, entityDef.DatabaseName, sql, true))
            //    {
            //        while (reader.Read())
            //        {
            //            deletedIds.Add(reader.GetInt32(0));
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
    }
}
