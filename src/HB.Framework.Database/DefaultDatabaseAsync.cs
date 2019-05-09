using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HB.Framework.Database.Entity;
using HB.Framework.Database.SQL;
using HB.Framework.Database.Transaction;
using Microsoft.Extensions.Logging;

namespace HB.Framework.Database
{
    public partial class DefaultDatabase
    {
        #region 单表查询, Select, From, Where

        public Task<T> ScalarAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster)
                .ContinueWith(t=> {
                    IList<T> lst = t.Result;

                    if (lst == null || lst.Count == 0)
                    {
                        return default;
                    }

                    if (lst.Count > 1)
                    {
                        _logger.LogCritical(0, "retrieve result not one, but many." + typeof(T).FullName, null);
                        return default;
                    }

                    return lst[0];
                }, TaskScheduler.Default);
        }

        public async Task<IList<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition, DatabaseTransactionContext transContext = null, bool useMaster = false)
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

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName, command, useMaster).ConfigureAwait(false);
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

        public async Task<IList<T>> RetrieveAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
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
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName, command, useMaster).ConfigureAwait(false);
                result = _modelMapper.ToList<T>(reader);
            }
            catch (DbException ex)
            {
                result = null;
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

        public Task<IList<T>> PageAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
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

            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext, useMaster);
        }

        public async Task<long> CountAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
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
                object countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster).ConfigureAwait(false);
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

        public Task<T> ScalarAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync(null, fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync(null, fromCondition, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Where

        public Task<IList<T>> RetrieveAllAsync<T>(DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, null, null, transContext, useMaster);
        }

        public Task<T> ScalarAsync<T>(WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(WhereExpression<T> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, null, null, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(WhereExpression<T> condition, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync(null, null, condition, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, null, null, transContext, useMaster);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T> ScalarAsync<T>(long id, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync<T>(t => t.Id == id && t.Deleted == false, transContext, useMaster);
        }

        //public Task<T> RetrieveScalaAsyncr<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster = false) where T : DatabaseEntity, new();
        public Task<T> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster = false) where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return ScalarAsync(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return RetrieveAsync(null, null, whereCondition, transContext, useMaster);
        }

        public Task<IList<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory).Where(whereExpr);

            return PageAsync(null, null, whereCondition, pageNumber, perPageCount, transContext, useMaster);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext, bool useMaster)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = new WhereExpression<T>(_databaseEngine, _entityDefFactory);
            whereCondition.Where(whereExpr);

            return CountAsync(null, null, whereCondition, transContext, useMaster);
        }

        #endregion

        #region 双表查询

        public async Task<IList<Tuple<TSource, TTarget>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);

            IList<Tuple<TSource, TTarget>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget>(fromCondition, whereCondition);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster).ConfigureAwait(false);
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

        public Task<IList<Tuple<TSource, TTarget>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<Tuple<TSource, TTarget>> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
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

        public async Task<IList<Tuple<TSource, TTarget1, TTarget2>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.And(t => t.Deleted == false)
                .And<TTarget1>(t => t.Deleted == false)
                .And<TTarget2>(t => t.Deleted == false);


            IList<Tuple<TSource, TTarget1, TTarget2>> result = null;
            IDataReader reader = null;
            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<TSource>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(fromCondition, whereCondition);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, useMaster).ConfigureAwait(false);
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

        public Task<IList<Tuple<TSource, TTarget1, TTarget2>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, DatabaseTransactionContext transContext, bool useMaster)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = new WhereExpression<TSource>(_databaseEngine, _entityDefFactory);
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext, useMaster);
        }

        public Task<Tuple<TSource, TTarget1, TTarget2>> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, DatabaseTransactionContext transContext, bool useMaster)
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
        public async Task<DatabaseResult> AddAsync<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
        {
            if (!item.IsValid())
            {
                //TODO: 给所有使用到IsValid（）方法的地方，都加上GetValidateErrorMessage输出
                return DatabaseResult.Fail($"entity check failed.{item.GetValidateErrorMessage()}");
            }

            DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();

            if (!entityDef.DatabaseWriteable)
            {
                return DatabaseResult.NotWriteable();
            }

            IDataReader reader = null;

            try
            {
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateAddCommand(item, "default"), true).ConfigureAwait(false);

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
        public async Task<DatabaseResult> DeleteAsync<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.GetDeleteCommand(condition, "default")).ConfigureAwait(false);

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
                _logger.LogCritical(ex.Message);
                return DatabaseResult.Fail(ex);
            }
        }

        /// <summary>
        ///  修改，建议每次修改前先select，并放置在一个事务中。
        ///  版本控制，如果item中Version未赋值，会无法更改
        /// </summary>
        public async Task<DatabaseResult> UpdateAsync<T>(T item, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, _sqlBuilder.CreateUpdateCommand(condition, item, "default")).ConfigureAwait(false);

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

        //TODO: 加锁

        /// <summary>
        /// 批量添加,返回新产生的ID列表
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Task<DatabaseResult> BatchAddAsync<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
        public Task<DatabaseResult> BatchUpdateAsync<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
        public Task<DatabaseResult> BatchDeleteAsync<T>(IList<T> items, string lastUser, DatabaseTransactionContext transContext) where T : DatabaseEntity, new()
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
