using System;
using System.Linq;
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
    internal partial class DefaultDatabase
    {
        #region 单表查询, Select, From, Where

        public Task<T> ScalarAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext)
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

        public async Task<IList<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(SelectExpression<TSelect> selectCondition, FromExpression<TFrom> fromCondition, WhereExpression<TWhere> whereCondition, TransactionContext transContext = null)
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
                whereCondition = NewWhere<TWhere>();
            }

            whereCondition.And(t => t.Deleted == false).And<TSelect>(ts=>ts.Deleted == false).And<TFrom>(tf=>tf.Deleted == false);

            #endregion

            IList<TSelect> result = null;
            IDataReader reader = null;
            DatabaseEntityDef selectDef = _entityDefFactory.GetDef<TSelect>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand(selectCondition, fromCondition, whereCondition);
                bindCommandTransaction(transContext, command);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName, command, transContext != null).ConfigureAwait(false);
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

        

        public async Task<IList<T>> RetrieveAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
			}

			if (whereCondition == null)
			{
				whereCondition = NewWhere<T>();
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

			IList<T> result = null;
            IDataReader reader = null;
            DatabaseEntityDef modelDef = _entityDefFactory.GetDef<T>();

            try
            {
                IDbCommand command = _sqlBuilder.CreateRetrieveCommand<T>(selectCondition, fromCondition, whereCondition);
                bindCommandTransaction(transContext, command);
                

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName, command, transContext != null).ConfigureAwait(false);
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

        public Task<IList<T>> PageAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
			}

			if (whereCondition == null)
			{
                whereCondition = NewWhere<T>();
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

			whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<T>(selectCondition, fromCondition, whereCondition, transContext);
        }

        public async Task<long> CountAsync<T>(SelectExpression<T> selectCondition, FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
			#region Argument Adjusting

			if (selectCondition != null)
			{
				selectCondition.Select(t => t.Id).Select(t => t.Deleted).Select(t => t.LastTime).Select(t => t.LastUser).Select(t => t.Version);
			}

			if (whereCondition == null)
			{
                whereCondition = NewWhere<T>();
			}

			whereCondition.And(t => t.Deleted == false);

			#endregion

            long count = -1;

			DatabaseEntityDef entityDef = _entityDefFactory.GetDef<T>();
            try
            {
                IDbCommand command = _sqlBuilder.CreateCountCommand(fromCondition, whereCondition);
                bindCommandTransaction(transContext, command);
                object countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null).ConfigureAwait(false);
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

        public Task<T> ScalarAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync(null, fromCondition, whereCondition, transContext);
        }

        public Task<IList<T>> RetrieveAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync(null, fromCondition, whereCondition, transContext);
        }

        public Task<IList<T>> PageAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return PageAsync(null, fromCondition, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(FromExpression<T> fromCondition, WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return CountAsync(null, fromCondition, whereCondition, transContext);
        }

        #endregion

        #region 单表查询, Where

        public Task<IList<T>> RetrieveAllAsync<T>(TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync<T>(null, null, null, transContext);
        }

        public Task<T> ScalarAsync<T>(WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync(null, null, whereCondition, transContext);
        }

        public Task<IList<T>> RetrieveAsync<T>(WhereExpression<T> whereCondition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return RetrieveAsync(null, null, whereCondition, transContext);
        }

        public Task<IList<T>> PageAsync<T>(WhereExpression<T> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return PageAsync(null, null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<IList<T>> PageAsync<T>(long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return PageAsync<T>(null, null, null, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(WhereExpression<T> condition, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return CountAsync(null, null, condition, transContext);
        }

        public Task<long> CountAsync<T>(TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return CountAsync<T>(null, null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T> ScalarAsync<T>(long id, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            return ScalarAsync<T>(t => t.Id == id && t.Deleted == false, transContext);
        }

        //public Task<T> RetrieveScalaAsyncr<T>(Expression<Func<T, bool>> whereExpr, DatabaseTransactionContext transContext = false) where T : DatabaseEntity, new();
        public Task<T> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext) where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = NewWhere<T>();
            whereCondition.Where(whereExpr);

            return ScalarAsync(null, null, whereCondition, transContext);
        }

        public Task<IList<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = NewWhere<T>();
            whereCondition.Where(whereExpr);

            return RetrieveAsync(null, null, whereCondition, transContext);
        }

        public Task<IList<T>> PageAsync<T>(Expression<Func<T, bool>> whereExpr, long pageNumber, long perPageCount, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = NewWhere<T>();

            return PageAsync(null, null, whereCondition, pageNumber, perPageCount, transContext);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext transContext)
            where T : DatabaseEntity, new()
        {
            WhereExpression<T> whereCondition = NewWhere<T>();
            whereCondition.Where(whereExpr);

            return CountAsync(null, null, whereCondition, transContext);
        }

        #endregion

        #region 双表查询

        public async Task<IList<Tuple<TSource, TTarget>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = NewWhere<TSource>();
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
                bindCommandTransaction(transContext, command);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null).ConfigureAwait(false);
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

        public Task<IList<Tuple<TSource, TTarget>>> PageAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = NewWhere<TSource>();
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext);
        }

        public Task<Tuple<TSource, TTarget>> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget : DatabaseEntity, new()
        {
            return RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext)
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

        public async Task<IList<Tuple<TSource, TTarget1, TTarget2>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = NewWhere<TSource>();
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
                bindCommandTransaction(transContext, command);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, command, transContext != null).ConfigureAwait(false);
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

        public Task<IList<Tuple<TSource, TTarget1, TTarget2>>> PageAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, long pageNumber, long perPageCount, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            if (whereCondition == null)
            {
                whereCondition = NewWhere<TSource>(); 
            }

            whereCondition.Limit((pageNumber - 1) * perPageCount, perPageCount);

            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext);
        }

        public Task<Tuple<TSource, TTarget1, TTarget2>> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource> whereCondition, TransactionContext transContext)
            where TSource : DatabaseEntity, new()
            where TTarget1 : DatabaseEntity, new()
            where TTarget2 : DatabaseEntity, new()
        {
            return RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext)
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
        public async Task<DatabaseResult> AddAsync<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
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
                IDbCommand dbCommand = _sqlBuilder.CreateAddCommand(item, "default");
                bindCommandTransaction(transContext, dbCommand);

                reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, entityDef.DatabaseName, dbCommand, true).ConfigureAwait(false);

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
        public async Task<DatabaseResult> DeleteAsync<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
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
            WhereExpression<T> condition = NewWhere<T>().Where(t => t.Id == id && t.Deleted == false && t.Version == version);

            try
            {
                IDbCommand dbCommand = _sqlBuilder.GetDeleteCommand(condition, "default");
                bindCommandTransaction(transContext, dbCommand);

                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, dbCommand).ConfigureAwait(false);

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
        public async Task<DatabaseResult> UpdateAsync<T>(T item, TransactionContext transContext) where T : DatabaseEntity, new()
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

            WhereExpression<T> condition = NewWhere<T>();

            long id = item.Id;
            long version = item.Version;

            condition.Where(t => t.Id == id).And(t => t.Deleted == false);

            //版本控制
            condition.And(t => t.Version == version);

            try
            {
                IDbCommand dbCommand = _sqlBuilder.CreateUpdateCommand(condition, item, "default");
                bindCommandTransaction(transContext, dbCommand);
                long rows = await _databaseEngine.ExecuteCommandNonQueryAsync(transContext?.Transaction, entityDef.DatabaseName, dbCommand).ConfigureAwait(false);

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

        #region 批量更改(Write)

        public async Task<DatabaseResult> BatchAddAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
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
                bindCommandTransaction(transContext, dbCommand);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
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
        /// 批量更改
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public async Task<DatabaseResult> BatchUpdateAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
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
                bindCommandTransaction(transContext, dbCommand);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
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

        public async Task<DatabaseResult> BatchDeleteAsync<T>(IEnumerable<T> items, string lastUser, TransactionContext transContext) where T : DatabaseEntity, new()
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
                bindCommandTransaction(transContext, dbCommand);
                reader = await _databaseEngine.ExecuteCommandReaderAsync(
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
                _logger.Error_BatchDelete_Thrown(ex, lastUser);
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
    }
}
