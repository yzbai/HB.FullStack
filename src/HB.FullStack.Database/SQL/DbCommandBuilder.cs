using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Implements;
using HB.FullStack.Database.SQL;

using Microsoft;

namespace HB.FullStack.Database.SQL
{
    internal partial class DbCommandBuilder : IDbCommandBuilder
    {
        private IDbModelDefFactory ModelDefFactory { get; set; }

        private ISQLExpressionVisitor ExpressionVisitor { get; set; }

        public DbCommandBuilder(IDbModelDefFactory modelDefFactory, ISQLExpressionVisitor expressionVisitor)
        {
            ModelDefFactory = modelDefFactory;
            ExpressionVisitor = expressionVisitor;
        }

        #region 条件构造

        public FromExpression<T> From<T>() where T : class, IDbModel
        {
            return new FromExpression<T>(ModelDefFactory, ExpressionVisitor);
        }

        public WhereExpression<T> Where<T>() where T : class, IDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, ExpressionVisitor);
        }

        public WhereExpression<T> Where<T>(string sqlFilter, params object[] filterParams) where T : class, IDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, ExpressionVisitor).Where(sqlFilter, filterParams);
        }

        public WhereExpression<T> Where<T>(Expression<Func<T, bool>> predicate) where T : class, IDbModel
        {
            DbModelDef modelDef = ModelDefFactory.GetDef<T>().ThrowIfNull(typeof(T).FullName);
            return new WhereExpression<T>(modelDef, ExpressionVisitor).Where(predicate);
        }

        #endregion

        #region Management

        public DbEngineCommand CreateTableCreateCommand(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength, int maxVarcharFieldLength, int maxMediumTextFieldLength)
        {
            string sql = SqlHelper.GetTableCreateSql(modelDef, addDropStatement, varcharDefaultLength, maxVarcharFieldLength, maxMediumTextFieldLength);

            return new DbEngineCommand(sql);
        }

        public DbEngineCommand CreateIsTableExistCommand(DbEngineType engineType, string tableName)
        {
            string sql = SqlHelper.GetIsTableExistSql(engineType);

            var parameters = new List<KeyValuePair<string, object>> {
                new KeyValuePair<string, object>("@tableName", tableName )
            };

            return new DbEngineCommand(sql, parameters);
        }

        public DbEngineCommand CreateSystemInfoRetrieveCommand(DbEngineType engineType)
        {
            string sql = SqlHelper.GetSystemInfoRetrieveSql(engineType);

            return new DbEngineCommand(sql);
        }

        public DbEngineCommand CreateSystemVersionSetCommand(DbEngineType engineType, string dbSchemaName, int version)
        {
            string sql;
            List<KeyValuePair<string, object>> parameters;

            if (version == 1)
            {
                sql = SqlHelper.GetSystemInfoCreateSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{SystemInfoNames.DATABASE_SCHEMA}", dbSchemaName) };
            }
            else
            {
                sql = SqlHelper.GetSystemInfoUpdateVersionSql(engineType);

                parameters = new List<KeyValuePair<string, object>> { new KeyValuePair<string, object>($"@{SystemInfoNames.VERSION}", version) };
            }

            return new DbEngineCommand(sql, parameters);
        }

        #endregion Management
    }
}