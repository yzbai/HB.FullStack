/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Implements;

using static System.FormattableString;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        public const string OLD_PROPERTY_VALUE_SUFFIX = "old";
        public const string NEW_PROPERTY_VALUE_SUFFIX = "new";

        public static readonly string DbParameterName_OldTimestamp = GetParameterized($"{nameof(ITimestamp.Timestamp)}_{OLD_PROPERTY_VALUE_SUFFIX}");
        public static readonly string DbParameterName_NewTimestamp = GetParameterized($"{nameof(ITimestamp.Timestamp)}_{NEW_PROPERTY_VALUE_SUFFIX}");
        public static readonly string DbParameterName_Timestamp = GetParameterized(nameof(ITimestamp.Timestamp));

        public static readonly string DbParameterName_LastUser = GetParameterized(nameof(BaseDbModel.LastUser));
        public static readonly string DbParameterName_Deleted = GetParameterized(nameof(BaseDbModel.Deleted));
        public static readonly string DbParameterName_Id = GetParameterized(nameof(DbModel2<long>.Id));

        #region Cache Sql

        private static readonly Dictionary<string, string> SqlCache = new Dictionary<string, string>();

        private static string GetCachedSqlKey(DbModelDef[] modelDefs, IList<string>? propertyNames, IList<object?>? otherParameters, [CallerMemberName] string caller = "")
        {
            ThrowIf.NullOrEmpty(caller, nameof(caller));

            StringBuilder builder = new StringBuilder(modelDefs[0].DbSchema.Name);

            foreach (DbModelDef modelDef in modelDefs)
            {
                builder.Append(modelDef.TableName);
                builder.Append('_');
            }

            if (propertyNames.IsNotNullOrEmpty())
            {
                foreach (string propertyName in propertyNames)
                {
                    builder.Append(propertyName);
                    builder.Append('_');
                }
            }

            builder.Append(caller);
            builder.Append('_');

            if (otherParameters.IsNotNullOrEmpty())
            {
                foreach (object? other in otherParameters)
                {
                    builder.Append(other);
                    builder.Append('_');
                }
            }

            return builder.ToString();
        }

        #endregion

        #region Batch Sql

        enum BatchSqlReturnType
        {
            None,
            ReturnFoundUpdateMatchedRows,
            ReturnLastInsertIds
        }

        private static string CreateBatchSql(BatchSqlReturnType returnType, DbModelDef modelDef, IList<object?> createSqlParameters, Func<int, object?, string> createSql)
        {
            //TODO: 将这一段反复重复的代码进行重构
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            StringBuilder innerBuilder = new StringBuilder();
            DbEngineType engineType = modelDef.EngineType;

            string returnSqlTemplate = returnType switch
            {
                BatchSqlReturnType.None => "",
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows => $" {TempTable_Insert_Id(tempTableName, FoundUpdateMatchedRows_Statement(engineType), engineType)}",
                BatchSqlReturnType.ReturnLastInsertIds => $" {TempTable_Insert_Id(tempTableName, LastInsertIdStatement(engineType), engineType)}",
                _ => throw new NotImplementedException($"Wrong Return SqlTemplate:{returnType}.")
            };

            for (int i = 0; i < createSqlParameters.Count; ++i)
            {
                innerBuilder.Append(createSql(i, createSqlParameters[i]));
                innerBuilder.Append(returnSqlTemplate);
            }

            if (returnType == BatchSqlReturnType.None)
            {
                return $@"{Transaction_Begin(engineType)}
                        {innerBuilder}
                        {Transaction_Commit(engineType)}";
            }
            else
            {
                return $@"{Transaction_Begin(engineType)}
                        {TempTable_Drop(tempTableName, engineType)}
                        {TempTable_Create_Id(tempTableName, engineType)}
                        {innerBuilder}
                        {TempTable_Select_Id(tempTableName, engineType)}
                        {TempTable_Drop(tempTableName, engineType)}
                        {Transaction_Commit(engineType)}";
            }
        }

        private static string CreateBatchSql(BatchSqlReturnType returnType, DbModelDef modelDef, int modelCount, Func<string> getTemplateSql, [CallerMemberName] string callerName = "")
        {
            string cacheKey = GetCachedSqlKey(new DbModelDef[] { modelDef }, null, new List<object?> { returnType }, callerName);

            if (!SqlCache.TryGetValue(cacheKey, out string? templateSql))
            {
                templateSql = getTemplateSql();
                SqlCache[cacheKey] = templateSql;
            }

            //TODO: 将这一段反复重复的代码进行重构
            string tempTableName = "t" + SecurityUtil.CreateUniqueToken();
            StringBuilder innerBuilder = new StringBuilder();
            DbEngineType engineType = modelDef.EngineType;

            string returnSqlTemplate = returnType switch
            {
                BatchSqlReturnType.None => "",
                BatchSqlReturnType.ReturnFoundUpdateMatchedRows => $" {TempTable_Insert_Id(tempTableName, FoundUpdateMatchedRows_Statement(engineType), engineType)}",
                BatchSqlReturnType.ReturnLastInsertIds => $" {TempTable_Insert_Id(tempTableName, LastInsertIdStatement(engineType), engineType)}",
                _ => throw new NotImplementedException($"Wrong Return SqlTemplate:{returnType}.")
            };

            for (int i = 0; i < modelCount; ++i)
            {
                innerBuilder.AppendFormat(templateSql, i);
                innerBuilder.Append(returnSqlTemplate);
            }

            if (returnType == BatchSqlReturnType.None)
            {
                return $@"{Transaction_Begin(engineType)}
                        {innerBuilder}
                        {Transaction_Commit(engineType)}";
            }
            else
            {
                return $@"{Transaction_Begin(engineType)}
                        {TempTable_Drop(tempTableName, engineType)}
                        {TempTable_Create_Id(tempTableName, engineType)}
                        {innerBuilder}
                        {TempTable_Select_Id(tempTableName, engineType)}
                        {TempTable_Drop(tempTableName, engineType)}
                        {Transaction_Commit(engineType)}";
            }
        }

        #endregion

        #region Sql Dialect

        /// <summary>
        /// 用于参数化的字符（@）,用于参数化查询
        /// </summary>
        public const string PARAMETERIZED_CHAR = "@";

        /// <summary>
        /// 用于引号化的字符(')，用于字符串
        /// </summary>
        public const string QUOTED_CHAR = "'";

        public const string DOUBLE_QUOTED_CHAR = QUOTED_CHAR + QUOTED_CHAR;

        public static string GetQuoted(string name)
        {
            return QUOTED_CHAR + name.Replace(QUOTED_CHAR, DOUBLE_QUOTED_CHAR, StringComparison.InvariantCulture) + QUOTED_CHAR;
        }

        public static string GetParameterized(string name)
        {
            return PARAMETERIZED_CHAR + name;
        }

        public static string GetReserved(string name, DbEngineType engineType)
        {
            string reservedChar = GetReservedChar(engineType);
            return reservedChar + name + reservedChar;
        }

        /// <summary>
        /// 用于专有化的字符（`）
        /// </summary>
        public static string GetReservedChar(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "`",
                DbEngineType.SQLite => @"""",
                _ => throw new NotSupportedException()
            };
        }

        private static readonly List<Type> _needQuotedTypes = new List<Type> { 
            typeof(string), 
            typeof(char), 
            typeof(Guid), 
            typeof(DateTimeOffset), 
            typeof(byte[]) };

        public static bool IsValueNeedQuoted(Type type)
        {
            Type trueType = Nullable.GetUnderlyingType(type) ?? type;

            if (trueType.IsEnum)
            {
                return true;
            }

            return _needQuotedTypes.Contains(type);
        }

        #endregion
    }
}