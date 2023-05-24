
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database
{
    public interface IDbCommandBuilder
    {
        

        #region Statements

        FromExpression<T> From<T>() where T : BaseDbModel;
        WhereExpression<T> Where<T>() where T : BaseDbModel;
        WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : BaseDbModel;
        WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : BaseDbModel;

        #endregion

        #region System

        DbEngineCommand CreateSystemInfoRetrieveCommand(DbEngineType engineType);

        /// <summary>
        /// 当前Connection下
        /// </summary>
        DbEngineCommand CreateIsTableExistCommand(DbEngineType engineType, string tableName);
        /// <summary>
        /// 如果tb_sys_info不存在则创建
        /// </summary>
        DbEngineCommand CreateSystemVersionSetCommand(DbEngineType engineType, string dbSchemaName, int version);
        DbEngineCommand CreateTableCreateCommand(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength, int maxVarcharFieldLength, int maxMediumTextFieldLength);

        #endregion

        #region Add

        DbEngineCommand CreateAddCommand<T>(DbModelDef modelDef, T model) where T : BaseDbModel;
        DbEngineCommand CreateBatchAddCommand<T>(DbModelDef modelDef, IList<T> models) where T : BaseDbModel;

        #endregion

        #region AddOrUpdate
        
        DbEngineCommand CreateAddOrUpdateCommand<T>(DbModelDef modelDef, T model, bool returnModel) where T : BaseDbModel;
        DbEngineCommand CreateBatchAddOrUpdateCommand<T>(DbModelDef modelDef, IList<T> models) where T : BaseDbModel;

        #endregion

        #region Retrieve

        DbEngineCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : BaseDbModel;
        
        DbEngineCommand CreateRetrieveCommand<T>(DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null) where T : BaseDbModel;

        DbEngineCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : BaseDbModel
            where T2 : BaseDbModel;
        DbEngineCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : BaseDbModel
            where T2 : BaseDbModel
            where T3 : BaseDbModel;
        DbEngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(
            FromExpression<TFrom>? fromCondition,
            WhereExpression<TWhere>? whereCondition,
            params DbModelDef[] returnModelDefs)
            where TSelect : BaseDbModel
            where TFrom :   BaseDbModel
            where TWhere : BaseDbModel;

        #endregion

        #region Update

        DbEngineCommand CreateUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, T model) where T : BaseDbModel;

        DbEngineCommand CreateUpdateTimestampCommand<T>(DbModelDef modelDef, T model, long oldTimestamp) where T : BaseDbModel;

        DbEngineCommand CreateBatchUpdateIgnoreConflictCheckCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : BaseDbModel;
        
        DbEngineCommand CreateBatchUpdateTimestampCommand<T>(DbModelDef modelDef, IList<T> models, IList<long> oldTimestamps) where T : BaseDbModel;

        #endregion

        #region UpdateProperties

        DbEngineCommand CreateUpdatePropertiesTimestampCommand(DbModelDef modelDef, TimestampUpdatePack updatePack, string lastUser);

        DbEngineCommand CreateBatchUpdatePropertiesTimestampCommand(DbModelDef modelDef, IList<TimestampUpdatePack> updatePacks, string lastUser);

        DbEngineCommand CreateUpdatePropertiesOldNewCompareCommand(DbModelDef modelDef, OldNewCompareUpdatePack updatePack, string lastUser);
        
        DbEngineCommand CreateBatchUpdatePropertiesOldNewCompareCommand(DbModelDef modelDef, IList<OldNewCompareUpdatePack> updatePacks, string lastUser);

        DbEngineCommand CreateUpdatePropertiesIgnoreConflictCheckCommand(DbModelDef modelDef, IgnoreConflictCheckUpdatePack updatePack, string lastUser);

        DbEngineCommand CreateBatchUpdatePropertiesIgnoreConflictCheckCommand(DbModelDef modelDef, IList<IgnoreConflictCheckUpdatePack> updatePack, string lastUser);

        #endregion

        #region Delete

        DbEngineCommand CreateDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, object id, string lastUser, bool trulyDelete, long? newTimestamp);
        DbEngineCommand CreateDeleteTimestampCommand(DbModelDef modelDef, object id, long timestamp, string lastUser, bool trulyDelete, long? newTimestamp);
        DbEngineCommand CreateDeleteOldNewCompareCommand<T>(DbModelDef modelDef, T model, string lastUser, bool trulyDelete, long? newTimestamp) where T : BaseDbModel;
        DbEngineCommand CreateBatchDeleteTimestampCommand(DbModelDef modelDef, IList<object> ids, IList<long> timestamps, string lastUser, bool trulyDelete, long? newTimestamp = null);
        DbEngineCommand CreateBatchDeleteIgnoreConflictCheckCommand(DbModelDef modelDef, IList<object> ids, string lastUser, bool trulyDeleted, long? newTimestamp = null);
        DbEngineCommand CreateBatchDeleteOldNewCompareCommand<T>(DbModelDef modelDef, IList<T> models, string lastUser, bool trulyDelete, long? newTimestamp = null) where T : BaseDbModel;
        DbEngineCommand CreateDeleteConditonCommand<T>(DbModelDef modelDef, WhereExpression<T> whereExpression, string lastUser, bool trulyDeleted) where T : BaseDbModel;

        #endregion
    }
}