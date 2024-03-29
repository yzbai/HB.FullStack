﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */


using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using HB.FullStack.Common;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Implements;

using static System.FormattableString;

namespace HB.FullStack.Database.SQL
{
    internal static class SqlHelper
    {
        public const string OLD_PROPERTY_VALUE_SUFFIX = "old";
        public const string NEW_PROPERTY_VALUES_SUFFIX = "new";

        public static readonly string DbParameterName_OldTimestamp = GetParameterized($"{nameof(TimestampDbModel.Timestamp)}_{OLD_PROPERTY_VALUE_SUFFIX}");
        public static readonly string DbParameterName_NewTimestamp = GetParameterized($"{nameof(TimestampDbModel.Timestamp)}_{NEW_PROPERTY_VALUES_SUFFIX}");
        public static readonly string DbParameterName_Timestamp = GetParameterized(nameof(TimestampDbModel.Timestamp));

        public static readonly string DbParameterName_LastUser = GetParameterized(nameof(DbModel.LastUser));
        public static readonly string DbParameterName_Deleted = GetParameterized(nameof(DbModel.Deleted));
        public static readonly string DbParameterName_Id = GetParameterized(nameof(ILongId.Id));

        /// <summary>
        /// 只用于客户端，没有做Timestamp检查
        /// </summary>
        public static string CreateAddOrUpdateSql(DbModelDef modelDef, bool returnModel, int number = 0)
        {
            StringBuilder addArgs = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder addValues = new StringBuilder();
            StringBuilder updatePairs = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (returnModel)
                {
                    selectArgs.Append(Invariant($"{propertyDef.DbReservedName},"));
                }

                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                addArgs.Append(Invariant($"{propertyDef.DbReservedName},"));
                addValues.Append(Invariant($"{propertyDef.DbParameterizedName}_{number},"));

                if (propertyDef.IsPrimaryKey)
                {
                    continue;
                }

                updatePairs.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            if (returnModel)
            {
                selectArgs.RemoveLast();
            }

            addValues.RemoveLast();
            addArgs.RemoveLast();
            updatePairs.RemoveLast();

            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;

            string sql = $"insert into {modelDef.DbTableReservedName}({addArgs}) values({addValues}) {OnDuplicateKeyUpdateStatement(modelDef.EngineType, primaryKeyProperty)} {updatePairs};";

            if (returnModel)
            {
                if (modelDef.IsIdAutoIncrement)
                {
                    sql += $"select {selectArgs} from {modelDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {GetLastInsertIdStatement(modelDef.EngineType)};";
                }
                else
                {
                    sql += $"select {selectArgs} from {modelDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {primaryKeyProperty.DbParameterizedName}_{number};";
                }
            }

            return sql;
        }

        public static string CreateAddModelSql(DbModelDef modelDef, bool returnId, int number = 0)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                args.Append(Invariant($"{propertyDef.DbReservedName},"));

                values.Append(Invariant($"{propertyDef.DbParameterizedName}_{number},"));
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId && modelDef.IsIdAutoIncrement ? $"select {GetLastInsertIdStatement(modelDef.EngineType)};" : string.Empty;

            return $"insert into {modelDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        /// <summary>
        /// 需要随后在Parameters中特别添加Timestamp_old_number的值
        /// </summary>
        public static string CreateUpdateModelSql(DbModelDef modelDef, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey /*|| propertyDef.SiteName == nameof(Model.CreateTime)*/)
                {
                    continue;
                }

                args.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            args.RemoveLast();

            StringBuilder where = new StringBuilder();

            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;
            DbModelPropertyDef deletedProperty = modelDef.GetDbPropertyDef(nameof(DbModel.Deleted))!;

            where.Append(Invariant($"{primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{number} AND "));
            where.Append(Invariant($"{deletedProperty.DbReservedName}=0 "));

            if (modelDef.IsTimestampDBModel)
            {
                //TODO: 提高效率。简化所有的Version、LastTime、LastUser、Deleted、Id字段的 Property读取和DbReservedName使用
                DbModelPropertyDef timestampProperty = modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))!;
                //where.Append(Invariant($" AND {timestampProperty.DbReservedName}={timestampProperty.DbParameterizedName}_{number} - 1 "));
                where.Append(Invariant($" AND {timestampProperty.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} "));
            }

            return $"UPDATE {modelDef.DbTableReservedName} SET {args} WHERE {where};";
        }

        /// <summary>
        /// 使用Timestamp乐观锁的Update-Fields。行粒度。
        /// </summary>
        public static string CreateUpdatePropertiesSql(DbModelDef modelDef, IEnumerable<string> propertyNames, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                if (/*propertyName == nameof(Model.CreateTime) || */propertyName == nameof(TimestampLongIdDbModel.Id))
                {
                    continue;
                }

                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(propertyName);

                if (propertyDef == null)
                {
                    throw DbExceptions.PropertyNotFound(modelDef.ModelFullName, propertyName);
                }

                args.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            args.RemoveLast();

            StringBuilder where = new StringBuilder();

            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;
            DbModelPropertyDef deletedProperty = modelDef.GetDbPropertyDef(nameof(DbModel.Deleted))!;

            where.Append(Invariant($"{primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{number} AND "));
            where.Append(Invariant($"{deletedProperty.DbReservedName}=0 "));

            if (modelDef.IsTimestampDBModel)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))!;
                
                where.Append(Invariant($" AND {timestampProperty.DbReservedName}={DbParameterName_Timestamp}_{OLD_PROPERTY_VALUE_SUFFIX}_{number} "));
            }

            return $"UPDATE {modelDef.DbTableReservedName} SET {args} WHERE {where};";
        }

        /// <summary>
        /// 使用新旧值比较乐观锁的update-fields.field粒度
        /// </summary>
        public static string CreateUpdatePropertiesUsingCompareSql(DbModelDef modelDef, IEnumerable<string> propertyNames, int number = 0)
        {
            DbModelPropertyDef primaryKeyProperty = modelDef.PrimaryKeyPropertyDef;
            DbModelPropertyDef deletedProperty = modelDef.GetDbPropertyDef(nameof(DbModel.Deleted))!;
            DbModelPropertyDef lastUserProperty = modelDef.GetDbPropertyDef(nameof(DbModel.LastUser))!;

            StringBuilder args = new StringBuilder();
            args.Append(Invariant($"{lastUserProperty.DbReservedName}={DbParameterName_LastUser}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));

            //如果是TimestampDBModel，强迫加上Timestamp字段
            if (modelDef.IsTimestampDBModel)
            {
                DbModelPropertyDef timestampProperty = modelDef.GetDbPropertyDef(nameof(TimestampDbModel.Timestamp))!;

                args.Append(Invariant($", {timestampProperty.DbReservedName}={DbParameterName_Timestamp}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));
            }

            StringBuilder where = new StringBuilder();

            where.Append(Invariant($" {primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{NEW_PROPERTY_VALUES_SUFFIX}_{number} "));
            where.Append(Invariant($" AND {deletedProperty.DbReservedName}=0 "));

            foreach (string propertyName in propertyNames)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(propertyName);

                if (propertyDef == null)
                {
                    throw DbExceptions.PropertyNotFound(modelDef.ModelFullName, propertyName);
                }

                //这里就不加了
                if (propertyName != nameof(TimestampDbModel.Timestamp))
                {
                    args.Append(Invariant($",{propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{NEW_PROPERTY_VALUES_SUFFIX}_{number}"));
                }

                where.Append(Invariant($" AND  {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{OLD_PROPERTY_VALUE_SUFFIX}_{number}"));
            }

            //TODO: 还是要查验一下found_rows的并发？
            string sql = $"UPDATE {modelDef.DbTableReservedName} SET {args} WHERE {where};";

            if (modelDef.IsTimestampDBModel)
            {
                //" SELECT {FoundUpdateMatchedRows_Statement(engineType)}, {timestampProperty.DbReservedName} FROM {modelDef.DbTableReservedName} WHERE {primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{newSuffix}_{number} AND {deletedProperty.DbReservedName}=0 ";
            }

            return sql;
        }

        //public static string CreateDeleteModelSql(DbModelDef modelDef, int number = 0)
        //{
        //    return CreateUpdateModelSql(modelDef, number);
        //}

        public static string CreateSelectModelSql(params DbModelDef[] modelDefs)
        {
            StringBuilder builder = new StringBuilder("SELECT ");

            foreach (DbModelDef modelDef in modelDefs)
            {
                string DbTableReservedName = modelDef.DbTableReservedName;

                foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
                {
                    builder.Append(Invariant($"{DbTableReservedName}.{propertyDef.DbReservedName},"));
                }
            }

            builder.RemoveLast();

            return builder.ToString();
        }

        /// <summary>
        /// 针对Client
        /// </summary>
        public static string CreateUpdateDeletedSql(DbModelDef modelDef, int number = 0)
        {
            DbModelPropertyDef deletedProperty = modelDef.GetDbPropertyDef(nameof(DbModel.Deleted))!;
            DbModelPropertyDef lastNameProperty = modelDef.GetDbPropertyDef(nameof(DbModel.LastUser))!;

            return $"update {modelDef.DbTableReservedName} set  {deletedProperty.DbReservedName}={deletedProperty.DbParameterizedName}_{number},{lastNameProperty.DbReservedName}={lastNameProperty.DbParameterizedName}_{number}";
        }

        public static string CreateDeleteSql(DbModelDef modelDef, int number = 0)
        {
            return $"delete from {modelDef.DbTableReservedName} ";
        }

        public static string CreateDeleteByPropertiesSql(DbModelDef modelDef, IEnumerable<string> propertyNames, int number = 0)
        {
            StringBuilder where = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                DbModelPropertyDef? propertyDef = modelDef.GetDbPropertyDef(propertyName);

                if (propertyDef == null)
                {
                    throw DbExceptions.PropertyNotFound(modelDef.ModelFullName, propertyName);
                }

                where.Append($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number} ");
                where.Append("AND");
            }

            where.RemoveLast(3);// "AND".Length

            return $"delete from {modelDef.DbTableReservedName} where {where};";
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

        /// <summary>
        /// 用于参数化的字符（@）,用于参数化查询
        /// </summary>
        public const string PARAMETERIZED_CHAR = "@";

        /// <summary>
        /// 用于引号化的字符(')，用于字符串
        /// </summary>
        public const string QUOTED_CHAR = "'";

        public static string GetQuoted(string name)
        {
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
            return QUOTED_CHAR + name.Replace(QUOTED_CHAR, QUOTED_CHAR + QUOTED_CHAR, Globals.Comparison) + QUOTED_CHAR;
#elif NETSTANDARD2_0
            return QUOTED_CHAR + name.Replace(QUOTED_CHAR, QUOTED_CHAR + QUOTED_CHAR) + QUOTED_CHAR;
#endif
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

        private static readonly List<Type> _needQuotedTypes = new List<Type> { typeof(string), typeof(char), typeof(Guid), typeof(DateTimeOffset), typeof(byte[]) };

        public static bool IsValueNeedQuoted(Type type)
        {
            Type trueType = Nullable.GetUnderlyingType(type) ?? type;

            if (trueType.IsEnum)
            {
                return true;
            }

            return _needQuotedTypes.Contains(type);
        }

        public static bool IsDbFieldNeedLength(DbModelPropertyDef propertyDef, DbEngineType engineType)
        {
            DbType dbType = DbPropertyConvert.PropertyTypeToDbType(propertyDef, engineType);

            return dbType == DbType.String
                || dbType == DbType.StringFixedLength
                || dbType == DbType.AnsiString
                || dbType == DbType.AnsiStringFixedLength
                || dbType == DbType.VarNumeric;
        }

        public static string TempTable_Insert_Id(string tempTableName, string value, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                DbEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => throw new NotSupportedException()
            };
        }

        public static string TempTable_Select_Id(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"select `id` from `{tempTableName}`;",
                DbEngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Drop(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                DbEngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Create_Id(string tempTableName, DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                DbEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        //TODO: sqlite，如果在Batch语句里加上Begin，那么会引起SQLite Error 1: 'cannot start a transaction within a transaction'.
        //为什么？
        public static string Transaction_Begin(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Begin;",
                //DbEngineType.SQLite => "Begin;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Commit(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Commit;",
                //DbEngineType.SQLite => "Commit;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Rollback(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "Rollback;",
                //DbEngineType.SQLite => "Rollback;",
                DbEngineType.SQLite => "",
                _ => throw new NotImplementedException(),
            };
        }

        public static string FoundUpdateMatchedRows_Statement(DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回匹配到的
                //row_count() 返回真正被修改的

                DbEngineType.MySQL => " found_rows() ",//$"row_count()", // $" found_rows() ",
                DbEngineType.SQLite => " changes() ",//sqlite不返回真正受影响的，只返回匹配的
                _ => throw new NotImplementedException(),
            };
        }

        public static string FoundDeletedRows_Statement(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => " row_count() ",
                DbEngineType.SQLite => " changes() ",
                _ => throw new NotImplementedException(),
            };
        }

        public static string GetLastInsertIdStatement(DbEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DbEngineType.SQLite => "last_insert_rowid()",
                DbEngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        public static string GetOrderBySqlUtilInStatement(string quotedColName, string[] ins, DbEngineType databaseEngineType)
        {
            if (databaseEngineType == DbEngineType.MySQL)
            {
                return $" ORDER BY FIELD({quotedColName}, {ins.ToJoinedString(",")}) ";
            }
            else if (databaseEngineType == DbEngineType.SQLite)
            {
                StringBuilder orderCaseBuilder = new StringBuilder(" ORDER BY CASE ");

                orderCaseBuilder.Append(quotedColName);

                for (int i = 0; i < ins.Length; ++i)
                {
                    orderCaseBuilder.Append(Invariant($" when {ins[i]} THEN {i} "));
                }

                orderCaseBuilder.Append(" END ");

                return orderCaseBuilder.ToString();
            }

            throw new NotSupportedException();
        }

        public static string SQLite_Table_Create_Statement(DbModelDef modelDef, bool addDropStatement)
        {
            StringBuilder propertyInfoSql = new StringBuilder();
            StringBuilder indexSqlBuilder = new StringBuilder();

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                string dbTypeStatement = DbPropertyConvert.PropertyTypeToDbTypeStatement(propertyDef, DbEngineType.SQLite);

                string nullable = propertyDef.IsNullable ? "" : " NOT NULL ";

                string unique = propertyDef.IsUnique /*&& !propertyDef.IsForeignKey*/ && !propertyDef.IsAutoIncrementPrimaryKey ? " UNIQUE " : "";

                string primaryStatement = propertyDef.IsPrimaryKey ? " PRIMARY KEY " : "";

                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    primaryStatement += " AUTOINCREMENT ";
                }

                propertyInfoSql.Append(Invariant($" {propertyDef.DbReservedName} {dbTypeStatement} {primaryStatement} {nullable} {unique} ,"));

                //索引
                if (!propertyDef.IsUnique && !propertyDef.IsAutoIncrementPrimaryKey && (propertyDef.IsForeignKey || propertyDef.IsIndexNeeded))
                {
                    indexSqlBuilder.Append(Invariant($" create index {modelDef.TableName}_{propertyDef.Name}_index on {modelDef.DbTableReservedName} ({propertyDef.DbReservedName}); "));
                }
            }

            propertyInfoSql.Remove(propertyInfoSql.Length - 1, 1);

            string dropStatement = addDropStatement ? $"Drop table if exists {modelDef.DbTableReservedName};" : string.Empty;

            string tableCreateSql = $"{dropStatement} CREATE TABLE {modelDef.DbTableReservedName} ({propertyInfoSql});{indexSqlBuilder}";

            return tableCreateSql;
        }

        public static string MySQL_Table_Create_Statement(
            DbModelDef modelDef, 
            bool addDropStatement, 
            int varcharDefaultLength, 
            int maxVarcharFieldLength, 
            int maxMediumTextFieldLength)
        {
            StringBuilder propertySqlBuilder = new StringBuilder();
            StringBuilder indexSqlBuilder = new StringBuilder();

            DbModelPropertyDef? primaryKeyPropertyDef = null;

            foreach (DbModelPropertyDef propertyDef in modelDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey)
                {
                    primaryKeyPropertyDef = propertyDef;
                }

                string dbTypeStatement = DbPropertyConvert.PropertyTypeToDbTypeStatement(propertyDef, DbEngineType.MySQL);

                int length = 0;

#if NETSTANDARD2_1 || NET6_0_OR_GREATER
                if (IsDbFieldNeedLength(propertyDef, DbEngineType.MySQL) && !dbTypeStatement.Contains('(', StringComparison.Ordinal))
#endif
#if NETSTANDARD2_0
                if (IsDbFieldNeedLength(propertyDef, EngineType.MySQL) && !dbTypeStatement.Contains("("))
#endif
                {
                    if (propertyDef.DbMaxLength == null || propertyDef.DbMaxLength == 0)
                    {
                        length = varcharDefaultLength;
                    }
                    else
                    {
                        length = propertyDef.DbMaxLength.Value;
                    }
                }

                if (length >= maxVarcharFieldLength) //因为utf8mb4编码，一个汉字4个字节
                {
                    dbTypeStatement = "MEDIUMTEXT";
                }

                if (length >= maxMediumTextFieldLength)
                {
                    throw DbExceptions.ModelError(propertyDef.ModelDef.ModelFullName, propertyDef.Name, "字段长度太长");
                }

                //if (propertyDef.IsLengthFixed )
                //{
                //	dbTypeStatement = "CHAR";
                //}

                string lengthStatement = (length == 0 || dbTypeStatement == "MEDIUMTEXT") ? "" : "(" + length + ")";
                string nullableStatement = propertyDef.IsNullable == true ? "" : " NOT NULL ";
                string autoIncrementStatement = propertyDef.IsAutoIncrementPrimaryKey ? "AUTO_INCREMENT" : "";
                string uniqueStatement = !propertyDef.IsPrimaryKey && !propertyDef.IsForeignKey && propertyDef.IsUnique ? " UNIQUE " : "";

                propertySqlBuilder.Append(Invariant($" {propertyDef.DbReservedName} {dbTypeStatement}{lengthStatement} {nullableStatement} {autoIncrementStatement} {uniqueStatement},"));

                //判断索引
                if (propertyDef.IsForeignKey || propertyDef.IsIndexNeeded)
                {
                    indexSqlBuilder.Append(Invariant($" INDEX {propertyDef.Name}_index ({propertyDef.DbReservedName}), "));
                }
            }

            if (primaryKeyPropertyDef == null)
            {
                throw DbExceptions.ModelError(modelDef.ModelFullName, "", "no primary key");
            }

            string dropStatement = addDropStatement ? $"Drop table if exists {modelDef.DbTableReservedName};" : string.Empty;

            return $"{dropStatement} create table {modelDef.DbTableReservedName} ( {propertySqlBuilder} {indexSqlBuilder} PRIMARY KEY ({primaryKeyPropertyDef.DbReservedName})) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4;";
        }

        public static string GetTableCreateSql(DbModelDef modelDef, bool addDropStatement, int varcharDefaultLength, int maxVarcharFieldLength,
            int maxMediumTextFieldLength)
        {
            return modelDef.EngineType switch
            {
                DbEngineType.MySQL => MySQL_Table_Create_Statement(modelDef, addDropStatement, varcharDefaultLength, maxVarcharFieldLength, maxMediumTextFieldLength),
                DbEngineType.SQLite => SQLite_Table_Create_Statement(modelDef, addDropStatement),
                _ => throw new NotSupportedException()
            };
        }

        public static string OnDuplicateKeyUpdateStatement(DbEngineType engineType, DbModelPropertyDef primaryDef)
        {
            return engineType switch
            {
                DbEngineType.MySQL => "on duplicate key update",
                DbEngineType.SQLite => $"on conflict({primaryDef.DbReservedName}) do update set",
                _ => throw new NotSupportedException()
            };
        }

        public static string GetIsTableExistSql(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => MySqlIsTableExistsStatement,
                DbEngineType.SQLite => SqliteIsTableExistsStatement,
                _ => throw new NotSupportedException()
            };
        }

        public static string GetSystemInfoRetrieveSql(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => MySqlTbSysInfoRetrieve,
                DbEngineType.SQLite => SqliteTbSysinfoRetrieve,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoUpdateVersionSql(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => MySqlTbSysInfoUpdateVersion,
                DbEngineType.SQLite => SqliteTbSysinfoUpdateVersion,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoCreateSql(DbEngineType engineType)
        {
            return engineType switch
            {
                DbEngineType.MySQL => MySqlTbSysinfoCreate,
                DbEngineType.SQLite => SqliteTbSysinfoCreate,
                _ => string.Empty
            };
        }

        private const string MySqlTbSysinfoCreate =
$@"CREATE TABLE `tb_sys_info` (
`Id` int (11) NOT NULL AUTO_INCREMENT,
`Name` varchar(100) DEFAULT NULL,
`Value` varchar(1024) DEFAULT NULL,
PRIMARY KEY(`Id`),
UNIQUE KEY `Name_UNIQUE` (`Name`)
);
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('{SystemInfoNames.VERSION}', '1');
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('{SystemInfoNames.DATABASE_SCHEMA}', @{SystemInfoNames.DATABASE_SCHEMA});";

        private const string MySqlTbSysInfoUpdateVersion = $@"UPDATE `tb_sys_info` SET `Value` = @Value WHERE `Name` = '{SystemInfoNames.VERSION}';";

        private const string MySqlTbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private const string MySqlIsTableExistsStatement = "SELECT count(1) FROM information_schema.TABLES WHERE table_name =@tableName and table_schema=Database();";

        private const string SqliteTbSysinfoCreate =
$@"CREATE TABLE ""tb_sys_info"" (
""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
""Name"" TEXT UNIQUE,
""Value"" TEXT
);
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('{SystemInfoNames.VERSION}', '1');
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('{SystemInfoNames.DATABASE_SCHEMA}', @{SystemInfoNames.DATABASE_SCHEMA});";

        private const string SqliteTbSysinfoUpdateVersion = $@"UPDATE ""tb_sys_info"" SET ""Value"" = @Value WHERE ""Name"" = '{SystemInfoNames.VERSION}';";

        private const string SqliteTbSysinfoRetrieve = @"SELECT * FROM ""tb_sys_info"";";

        private const string SqliteIsTableExistsStatement = "SELECT count(1) FROM sqlite_master where type='table' and name=@tableName;";
    }
}