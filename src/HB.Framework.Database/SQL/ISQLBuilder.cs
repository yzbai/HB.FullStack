using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;

namespace HB.Framework.Database.SQL
{
    public interface ISQLBuilder
    {
        IDbCommand CreateAddCommand<T>(T domain, string lastUser) where T : DatabaseEntity, new();
        IDbCommand CreateCountCommand<T>(From<T> fromCondition = null, Where<T> whereCondition = null) where T : DatabaseEntity, new();
        
        IDbCommand CreateUpdateCommand<T>(Where<T> condition, T domain, string lastUser) where T : DatabaseEntity, new();
        IDbCommand CreateUpdateKeyCommand<T>(Where<T> condition, string[] keys, object[] values, string lastUser) where T : DatabaseEntity, new();
        string GetBatchAddStatement<T>(IList<T> domains, string lastUser) where T : DatabaseEntity;
        string GetBatchDeleteStatement<T>(IList<T> domains, string lastUser) where T : DatabaseEntity;
        string GetBatchUpdateStatement<T>(IList<T> domains, string lastUser) where T : DatabaseEntity;
        string GetCreateStatement(Type type, bool addDropStatement);
        IDbCommand GetDeleteCommand<T>(Where<T> condition, string lastUser) where T : DatabaseEntity, new();

        Select<T> NewSelect<T>() where T : DatabaseEntity, new();

        From<T> NewFrom<T>() where T : DatabaseEntity, new();

        Where<T> NewWhere<T>() where T : DatabaseEntity, new();


        IDbCommand CreateRetrieveCommand<T>(Select<T> selectCondition = null, From<T> fromCondition = null, Where<T> whereCondition = null) 
            where T : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<T1, T2>(From<T1> fromCondition, Where<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<T1, T2, T3>(From<T1> fromCondition, Where<T1> whereCondition)
            where T1 : DatabaseEntity, new()
            where T2 : DatabaseEntity, new()
            where T3 : DatabaseEntity, new();

        IDbCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(Select<TSelect> selectCondition, From<TFrom> fromCondition, Where<TWhere> whereCondition)
            where TSelect : DatabaseEntity, new()
            where TFrom : DatabaseEntity, new()
            where TWhere : DatabaseEntity, new();
    }
}