using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder
    {
        public DbEngineCommand CreateRetrieveCommand<T>(DbModelDef modelDef, FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : BaseDbModel
        {
            return AssembleRetrieveCommand(
                SqlHelper.CreateSelectSql(modelDef), 
                fromCondition, 
                whereCondition);
        }

        public DbEngineCommand CreateCountCommand<T>(FromExpression<T>? fromCondition = null, WhereExpression<T>? whereCondition = null)
            where T : BaseDbModel
        {
            return AssembleRetrieveCommand("SELECT COUNT(1) ", fromCondition, whereCondition);
        }

        public DbEngineCommand CreateRetrieveCommand<T1, T2>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : BaseDbModel
            where T2 : BaseDbModel
        {
            return AssembleRetrieveCommand(
                SqlHelper.CreateSelectSql(returnModelDefs),
                fromCondition,
                whereCondition);
        }

        public DbEngineCommand CreateRetrieveCommand<T1, T2, T3>(FromExpression<T1> fromCondition, WhereExpression<T1> whereCondition, params DbModelDef[] returnModelDefs)
            where T1 : BaseDbModel
            where T2 : BaseDbModel
            where T3 : BaseDbModel
        {
            return AssembleRetrieveCommand(
                SqlHelper.CreateSelectSql(returnModelDefs),
                fromCondition,
                whereCondition);
        }

        public DbEngineCommand CreateRetrieveCommand<TSelect, TFrom, TWhere>(FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition, params DbModelDef[] returnModelDefs)
            where TSelect : BaseDbModel
            where TFrom : BaseDbModel
            where TWhere : BaseDbModel
        {
            return AssembleRetrieveCommand(
                SqlHelper.CreateSelectSql(returnModelDefs),
                fromCondition,
                whereCondition);
        }

        private DbEngineCommand AssembleRetrieveCommand<TFrom, TWhere>(string selectSql, FromExpression<TFrom>? fromCondition, WhereExpression<TWhere>? whereCondition)
            where TFrom : BaseDbModel
            where TWhere : BaseDbModel
        {
            StringBuilder sqlBuilder = new StringBuilder(selectSql);
            List<KeyValuePair<string, object>> parameters = new List<KeyValuePair<string, object>>();

            fromCondition ??= From<TFrom>();

            sqlBuilder.Append(fromCondition.ToStatement());
            parameters.AddRange(fromCondition.GetParameters());

            if (whereCondition != null)
            {
                sqlBuilder.Append(whereCondition.ToStatement());

                parameters.AddRange(whereCondition.GetParameters());
            }

            return new DbEngineCommand(sqlBuilder.ToString(), parameters);
        }
    }
}
