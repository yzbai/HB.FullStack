using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    partial class DefaultDatabase
    {
        #region 单表查询 From, Where

        public async Task<T?> ScalarAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            IEnumerable<T> lst = await RetrieveAsync(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                throw DatabaseExceptions.FoundTooMuch(type: typeof(T).FullName, from: fromCondition?.ToStatement(), where: whereCondition?.ToStatement(_databaseEngine.EngineType));
            }

            return lst.ElementAt(0);
        }

        public async Task<IEnumerable<TSelect>> RetrieveAsync<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, TransactionContext? transContext = null)
            where TSelect : DbModel, new()
            where TFrom : DbModel, new()
            where TWhere : DbModel, new()
        {
            whereCondition ??= Where<TWhere>();

            DbModelDef selectDef = ModelDefFactory.GetDef<TSelect>()!;
            DbModelDef fromDef = ModelDefFactory.GetDef<TFrom>()!;
            DbModelDef whereDef = ModelDefFactory.GetDef<TWhere>()!;

            whereCondition.And($"{whereDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {selectDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {fromDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateRetrieveCommand<TSelect, TFrom, TWhere>(EngineType, fromCondition, whereCondition, selectDef);

                using IDataReader reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, selectDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);

                return reader.ToDbModels<TSelect>(_databaseEngine.EngineType, ModelDefFactory, selectDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: selectDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<IEnumerable<T>> RetrieveAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            whereCondition ??= Where<T>();

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            whereCondition.And($"{modelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateRetrieveCommand(EngineType, modelDef, fromCondition, whereCondition);

                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, modelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToDbModels<T>(_databaseEngine.EngineType, ModelDefFactory, modelDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<long> CountAsync<T>(FromExpression<T>? fromCondition, WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            whereCondition ??= Where<T>();

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            whereCondition.And($"{modelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");

            try
            {
                EngineCommand command = DbCommandBuilder.CreateCountCommand(EngineType, fromCondition, whereCondition);
                object? countObj = await _databaseEngine.ExecuteCommandScalarAsync(transContext?.Transaction, modelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return System.Convert.ToInt32(countObj, GlobalSettings.Culture);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: modelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        #endregion

        #region 单表查询, Where

        public Task<IEnumerable<T>> RetrieveAllAsync<T>(TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            WhereExpression<T> where = Where<T>().AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync(null, where, transContext);
        }

        public Task<T?> ScalarAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(WhereExpression<T>? whereCondition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<long> CountAsync<T>(WhereExpression<T>? condition, TransactionContext? transContext)
            where T : DbModel, new()
        {
            return CountAsync(null, condition, transContext);
        }

        public Task<long> CountAsync<T>(TransactionContext? transContext)
            where T : DbModel, new()
        {
            return CountAsync<T>(null, null, transContext);
        }

        #endregion

        #region 单表查询, Expression Where

        public Task<T?> ScalarAsync<T>(long id, TransactionContext? transContext)
            where T : DbModel, ILongId, new()
        {
            WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(ILongId.Id), EngineType)}={{0}}", id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Guid id, TransactionContext? transContext)
            where T : DbModel, IGuidId, new()
        {
            //WhereExpression<T> where = Where<T>($"{SqlHelper.GetReserved(nameof(TimestampGuidDbModel.Id), EngineType)}={{0}}", guid);
            WhereExpression<T> where = Where<T>(t => t.Id == id);

            return ScalarAsync(where, transContext);
        }

        public Task<T?> ScalarAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext) where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return ScalarAsync(null, whereCondition, transContext);
        }

        public Task<IEnumerable<T>> RetrieveAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr).AddOrderAndLimits(page, perPage, orderBy);

            return RetrieveAsync(null, whereCondition, transContext);
        }

        public Task<long> CountAsync<T>(Expression<Func<T, bool>> whereExpr, TransactionContext? transContext)
            where T : DbModel, new()
        {
            WhereExpression<T> whereCondition = Where(whereExpr);

            return CountAsync(null, whereCondition, transContext);
        }

        //TODO: orderby 添加对 desc的支持
        /// <summary>
        /// 根据给出的外键值获取 page从0开始
        /// </summary>
        public async Task<IEnumerable<T>> RetrieveByForeignKeyAsync<T>(Expression<Func<T, object>> foreignKeyExp, object foreignKeyValue, TransactionContext? transactionContext, int? page, int? perPage, string? orderBy)
            where T : DbModel, new()
        {
            string foreignKeyName = ((MemberExpression)foreignKeyExp.Body).Member.Name;

            DbModelDef modelDef = ModelDefFactory.GetDef<T>()!;

            DbModelPropertyDef? foreignKeyProperty = modelDef.GetPropertyDef(foreignKeyName);

            if (foreignKeyProperty == null || !foreignKeyProperty.IsForeignKey)
            {
                throw DatabaseExceptions.NoSuchForeignKey(modelDef.ModelFullName, foreignKeyName);
            }

            Type foreignKeyValueType = foreignKeyValue.GetType();

            if (foreignKeyValueType != typeof(long) && foreignKeyValueType != typeof(Guid))
            {
                throw DatabaseExceptions.KeyValueNotLongOrGuid(modelDef.ModelFullName, foreignKeyName, foreignKeyValue, foreignKeyValueType.FullName);
            }

            WhereExpression<T> where = Where<T>($"{foreignKeyName}={{0}}", foreignKeyValue)
                .AddOrderAndLimits(page, perPage, orderBy);

            return await RetrieveAsync(where, transactionContext).ConfigureAwait(false);
        }

        #endregion

        #region 双表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget?>>> RetrieveAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget : DbModel, new()
        {
            whereCondition ??= Where<TSource>();

            DbModelDef sourceModelDef = ModelDefFactory.GetDef<TSource>()!;
            DbModelDef targetModelDef = ModelDefFactory.GetDef<TTarget>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget>(EngineType, fromCondition, whereCondition, sourceModelDef, targetModelDef);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceModelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToDbModels<TSource, TTarget>(_databaseEngine.EngineType, ModelDefFactory, sourceModelDef, targetModelDef);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceModelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget?>?> ScalarAsync<TSource, TTarget>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget : DbModel, new()
        {
            IEnumerable<Tuple<TSource, TTarget?>> lst = await RetrieveAsync<TSource, TTarget>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                throw DatabaseExceptions.FoundTooMuch(typeof(TSource).FullName, from: fromCondition?.ToStatement(), where: whereCondition?.ToStatement(_databaseEngine.EngineType));
            }

            return lst.ElementAt(0);
        }

        #endregion

        #region 三表查询

        public async Task<IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>>> RetrieveAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget1 : DbModel, new()
            where TTarget2 : DbModel, new()
        {
            whereCondition ??= Where<TSource>();

            DbModelDef sourceModelDef = ModelDefFactory.GetDef<TSource>()!;
            DbModelDef targetModelDef1 = ModelDefFactory.GetDef<TTarget1>()!;
            DbModelDef targetModelDef2 = ModelDefFactory.GetDef<TTarget2>()!;

            switch (fromCondition.JoinType)
            {
                case SqlJoinType.LEFT:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false);
                    break;

                case SqlJoinType.RIGHT:
                    whereCondition.And($"{targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.INNER:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;

                case SqlJoinType.FULL:
                    break;

                case SqlJoinType.CROSS:
                    whereCondition.And($"{sourceModelDef.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef1.DbTableReservedName}.{_deletedPropertyReservedName}=0 and {targetModelDef2.DbTableReservedName}.{_deletedPropertyReservedName}=0");
                    //whereCondition.And(t => t.Deleted == false).And<TTarget1>(t => t.Deleted == false).And<TTarget2>(t => t.Deleted == false);
                    break;
            }

            try
            {
                var command = DbCommandBuilder.CreateRetrieveCommand<TSource, TTarget1, TTarget2>(EngineType, fromCondition, whereCondition, sourceModelDef, targetModelDef1, targetModelDef2);
                using var reader = await _databaseEngine.ExecuteCommandReaderAsync(transContext?.Transaction, sourceModelDef.DatabaseName!, command, transContext != null).ConfigureAwait(false);
                return reader.ToDbModels<TSource, TTarget1, TTarget2>(_databaseEngine.EngineType, ModelDefFactory, sourceModelDef, targetModelDef1, targetModelDef2);
            }
            catch (Exception ex) when (ex is not DatabaseException)
            {
                throw DatabaseExceptions.UnKown(type: sourceModelDef.ModelFullName, from: fromCondition?.ToStatement(), where: whereCondition.ToStatement(_databaseEngine.EngineType), innerException: ex);
            }
        }

        public async Task<Tuple<TSource, TTarget1?, TTarget2?>?> ScalarAsync<TSource, TTarget1, TTarget2>(FromExpression<TSource> fromCondition, WhereExpression<TSource>? whereCondition, TransactionContext? transContext)
            where TSource : DbModel, new()
            where TTarget1 : DbModel, new()
            where TTarget2 : DbModel, new()
        {
            IEnumerable<Tuple<TSource, TTarget1?, TTarget2?>> lst = await RetrieveAsync<TSource, TTarget1, TTarget2>(fromCondition, whereCondition, transContext).ConfigureAwait(false);

            if (lst.IsNullOrEmpty())
            {
                return null;
            }

            if (lst.Count() > 1)
            {
                throw DatabaseExceptions.FoundTooMuch(typeof(TSource).FullName, fromCondition.ToStatement(), whereCondition?.ToStatement(_databaseEngine.EngineType));
            }

            return lst.ElementAt(0);
        }

        #endregion
    }
}
