

using HB.FullStack.Common;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using static System.FormattableString;

namespace HB.FullStack.Database.SQL
{
    internal static class SqlHelper
    {
        /// <summary>
        /// 只用于客户端，IdGenEntity上,
        /// 没有Version检查, Version不增长
        /// </summary>
        /// <returns></returns>
        public static string CreateAddOrUpdateSql(EntityDef entityDef, EngineType engineType, bool returnId, int number = 0)
        {
            StringBuilder addArgs = new StringBuilder();
            StringBuilder selectArgs = new StringBuilder();
            StringBuilder addValues = new StringBuilder();
            StringBuilder updatePairs = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                if (returnId)
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

                if (propertyDef.Name == nameof(Entity.Version) || propertyDef.Name == nameof(Entity.CreateTime))
                {
                    continue;
                }

                updatePairs.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            EntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            //updatePairs.Append(Invariant($"{versionProperty.DbReservedName}={versionProperty.DbReservedName} + 1"));
            updatePairs.Append(Invariant($"{versionProperty.DbReservedName}={versionProperty.DbReservedName}"));

            if (returnId)
            {
                selectArgs.RemoveLast();
            }

            addValues.RemoveLast();
            addArgs.RemoveLast();

            EntityPropertyDef primaryKeyProperty = entityDef.PrimaryKeyPropertyDef;

            string sql = $"insert into {entityDef.DbTableReservedName}({addArgs}) values({addValues}) {OnDuplicateKeyUpdateStatement(engineType, primaryKeyProperty)} {updatePairs};";

            if (returnId)
            {
                if (entityDef.IsIdAutoIncrement)
                {
                    sql += $"select {selectArgs} from {entityDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {GetLastInsertIdStatement(engineType)};";
                }
                else
                {
                    sql += $"select {selectArgs} from {entityDef.DbTableReservedName} where {primaryKeyProperty.DbReservedName} = {primaryKeyProperty.DbParameterizedName}_{number};";
                }
            }

            return sql;
        }

        public static string CreateAddEntitySql(EntityDef entityDef, EngineType engineType, bool returnId, int number = 0)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
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

            string returnIdStatement = returnId && entityDef.IsIdAutoIncrement ? $"select {GetLastInsertIdStatement(engineType)};" : string.Empty;

            return $"insert into {entityDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        public static string CreateUpdateEntitySql(EntityDef entityDef, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey || propertyDef.Name == nameof(Entity.CreateTime))
                {
                    continue;
                }

                args.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            args.RemoveLast();

            StringBuilder where = new StringBuilder();

            EntityPropertyDef primaryKeyProperty = entityDef.PrimaryKeyPropertyDef;
            EntityPropertyDef deletedProperty = entityDef.GetPropertyDef(nameof(Entity.Deleted))!;
            EntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            where.Append(Invariant($"{primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{number} AND "));
            where.Append(Invariant($"{versionProperty.DbReservedName}={versionProperty.DbParameterizedName}_{number} - 1 AND "));
            where.Append(Invariant($"{deletedProperty.DbReservedName}=0"));

            return $"UPDATE {entityDef.DbTableReservedName} SET {args} WHERE {where};";
        }

        public static string CreateUpdateFieldsSql(EntityDef entityDef, IEnumerable<string> propertyNames, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (string propertyName in propertyNames)
            {
                if (propertyName == nameof(Entity.CreateTime) || propertyName == nameof(LongIdEntity.Id))
                {
                    continue;
                }

                EntityPropertyDef? propertyDef = entityDef.GetPropertyDef(propertyName);

                if (propertyDef == null)
                {
                    throw DatabaseExceptions.PropertyNotFound(entityDef.EntityFullName, propertyName);
                }

                args.Append(Invariant($" {propertyDef.DbReservedName}={propertyDef.DbParameterizedName}_{number},"));
            }

            args.RemoveLast();

            StringBuilder where = new StringBuilder();

            EntityPropertyDef primaryKeyProperty = entityDef.PrimaryKeyPropertyDef;
            EntityPropertyDef deletedProperty = entityDef.GetPropertyDef(nameof(Entity.Deleted))!;
            EntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            where.Append(Invariant($"{primaryKeyProperty.DbReservedName}={primaryKeyProperty.DbParameterizedName}_{number} AND "));
            where.Append(Invariant($"{versionProperty.DbReservedName}={versionProperty.DbParameterizedName}_{number} - 1 AND "));
            where.Append(Invariant($"{deletedProperty.DbReservedName}=0"));

            return $"UPDATE {entityDef.DbTableReservedName} SET {args} WHERE {where}";
        }

        public static string CreateDeleteEntitySql(EntityDef entityDef, int number = 0)
        {
            return CreateUpdateEntitySql(entityDef, number);
        }

        public static string CreateSelectEntitySql(params EntityDef[] entityDefs)
        {
            StringBuilder builder = new StringBuilder("SELECT ");

            foreach (EntityDef entityDef in entityDefs)
            {
                string DbTableReservedName = entityDef.DbTableReservedName;

                foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
                {
                    builder.Append(Invariant($"{DbTableReservedName}.{propertyDef.DbReservedName},"));
                }
            }

            builder.RemoveLast();

            return builder.ToString();
        }

        public static string CreateDeleteSql(EntityDef entityDef)
        {
            EntityPropertyDef deletedProperty = entityDef.GetPropertyDef(nameof(Entity.Deleted))!;
            EntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            return $"update {entityDef.DbTableReservedName} set {versionProperty.DbReservedName}={versionProperty.DbReservedName}+1, {deletedProperty.DbReservedName}=1";
        }

        /// <summary>
        /// 用于专有化的字符（`）
        /// </summary>
        public static string GetReservedChar(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => "`",
                EngineType.SQLite => @"""",
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
            return QUOTED_CHAR + name.Replace(QUOTED_CHAR, QUOTED_CHAR + QUOTED_CHAR, GlobalSettings.Comparison) + QUOTED_CHAR;
#elif NETSTANDARD2_0
            return QUOTED_CHAR + name.Replace(QUOTED_CHAR, QUOTED_CHAR + QUOTED_CHAR) + QUOTED_CHAR;
#endif
        }

        public static string GetParameterized(string name)
        {
            return PARAMETERIZED_CHAR + name;
        }

        public static string GetReserved(string name, EngineType engineType)
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

        public static bool IsDbFieldNeedLength(EntityPropertyDef propertyDef, EngineType engineType)
        {
            DbType dbType = TypeConvert.TypeToDbType(propertyDef, engineType);

            return dbType == DbType.String
                || dbType == DbType.StringFixedLength
                || dbType == DbType.AnsiString
                || dbType == DbType.AnsiStringFixedLength
                || dbType == DbType.VarNumeric;
        }

        public static string TempTable_Insert_Id(string tempTableName, string value, EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                EngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                EngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => throw new NotSupportedException()
            };
        }

        public static string TempTable_Select_Id(string tempTableName, EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                EngineType.MySQL => $"select `id` from `{tempTableName}`;",
                EngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Drop(string tempTableName, EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                EngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                EngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Create_Id(string tempTableName, EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                EngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                EngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        public static string Transaction_Begin(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => "Begin;",
                EngineType.SQLite => "Begin;",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Commit(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => "Commit;",
                EngineType.SQLite => "Commit;",
                _ => throw new NotImplementedException(),
            };
        }

        public static string Transaction_Rollback(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => "Rollback;",
                EngineType.SQLite => "Rollback;",
                _ => throw new NotImplementedException(),
            };
        }

        public static string FoundMatchedRows_Statement(EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回匹配到的
                //row_count() 返回真正被修改的
                
                EngineType.MySQL => " found_rows() ",//$"row_count()", // $" found_rows() ",
                EngineType.SQLite => $" changes() ",//sqlite不返回真正受影响的，只返回匹配的
                _ => "",
            };
        }

        public static string GetLastInsertIdStatement(EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                EngineType.SQLite => "last_insert_rowid()",
                EngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        public static string GetOrderBySqlUtilInStatement(string quotedColName, string[] ins, EngineType databaseEngineType)
        {
            if (databaseEngineType == EngineType.MySQL)
            {
                return $" ORDER BY FIELD({quotedColName}, {ins.ToJoinedString(",")}) ";
            }
            else if (databaseEngineType == EngineType.SQLite)
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

        public static string SQLite_Table_Create_Statement(EntityDef entityDef, bool addDropStatement)
        {
            StringBuilder propertyInfoSql = new StringBuilder();
            StringBuilder indexSqlBuilder = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, EngineType.SQLite);

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
                    indexSqlBuilder.Append(Invariant($" create index {entityDef.TableName}_{propertyDef.Name}_index on {entityDef.DbTableReservedName} ({propertyDef.DbReservedName}); "));
                }
            }

            propertyInfoSql.Remove(propertyInfoSql.Length - 1, 1);

            string dropStatement = addDropStatement ? $"Drop table if exists {entityDef.DbTableReservedName};" : string.Empty;

            string tableCreateSql = $"{dropStatement} CREATE TABLE {entityDef.DbTableReservedName} ({propertyInfoSql});{indexSqlBuilder}";

            return tableCreateSql;
        }

        public static string MySQL_Table_Create_Statement(EntityDef entityDef, bool addDropStatement, int varcharDefaultLength)
        {
            StringBuilder propertySqlBuilder = new StringBuilder();
            StringBuilder indexSqlBuilder = new StringBuilder();

            EntityPropertyDef? primaryKeyPropertyDef = null;

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                if (propertyDef.IsPrimaryKey)
                {
                    primaryKeyPropertyDef = propertyDef;
                }

                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, EngineType.MySQL);

                int length = 0;

#if NETSTANDARD2_1 || NET6_0_OR_GREATER
                if (IsDbFieldNeedLength(propertyDef, EngineType.MySQL) && !dbTypeStatement.Contains('(', StringComparison.Ordinal))
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

                if (length >= DefaultLengthConventions.MAX_VARCHAR_LENGTH) //因为utf8mb4编码，一个汉字4个字节
                {
                    dbTypeStatement = "MEDIUMTEXT";
                }

                if (length >= DefaultLengthConventions.MAX_MEDIUM_TEXT_LENGTH)
                {
                    throw DatabaseExceptions.EntityError(propertyDef.EntityDef.EntityFullName, propertyDef.Name, "字段长度太长");
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
                throw DatabaseExceptions.EntityError(entityDef.EntityFullName, "", "no primary key");
            }

            string dropStatement = addDropStatement ? $"Drop table if exists {entityDef.DbTableReservedName};" : string.Empty;

            return $"{dropStatement} create table {entityDef.DbTableReservedName} ( {propertySqlBuilder} {indexSqlBuilder} PRIMARY KEY ({primaryKeyPropertyDef.DbReservedName})) ENGINE=InnoDB  DEFAULT CHARSET=utf8mb4;";
        }

        public static string GetTableCreateSql(EntityDef entityDef, bool addDropStatement, int varcharDefaultLength, EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySQL_Table_Create_Statement(entityDef, addDropStatement, varcharDefaultLength),
                EngineType.SQLite => SQLite_Table_Create_Statement(entityDef, addDropStatement),
                _ => throw new NotSupportedException()
            };
        }

        public static string OnDuplicateKeyUpdateStatement(EngineType engineType, EntityPropertyDef primaryDef)
        {
            return engineType switch
            {
                EngineType.MySQL => "on duplicate key update",
                EngineType.SQLite => $"on conflict({primaryDef.DbReservedName}) do update set",
                _ => throw new NotSupportedException()
            };
        }

        public static string GetIsTableExistSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySqlIsTableExistsStatement,
                EngineType.SQLite => SqliteIsTableExistsStatement,
                _ => throw new NotSupportedException()
            };
        }

        public static string GetSystemInfoRetrieveSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySqlTbSysInfoRetrieve,
                EngineType.SQLite => SqliteTbSysinfoRetrieve,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoUpdateVersionSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySqlTbSysInfoUpdateVersion,
                EngineType.SQLite => SqliteTbSysinfoUpdateVersion,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoCreateSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySqlTbSysinfoCreate,
                EngineType.SQLite => SqliteTbSysinfoCreate,
                _ => string.Empty
            };
        }

        private const string MySqlTbSysinfoCreate =
@"CREATE TABLE `tb_sys_info` (
`Id` int (11) NOT NULL AUTO_INCREMENT,
`Name` varchar(100) DEFAULT NULL,
`Value` varchar(1024) DEFAULT NULL,
PRIMARY KEY(`Id`),
UNIQUE KEY `Name_UNIQUE` (`Name`)
);
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('Version', '1');
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('DatabaseName', @databaseName);";

        private const string MySqlTbSysInfoUpdateVersion = @"UPDATE `tb_sys_info` SET `Value` = @Value WHERE `Name` = 'Version';";

        private const string MySqlTbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private const string MySqlIsTableExistsStatement = "SELECT count(1) FROM information_schema.TABLES WHERE table_name =@tableName and table_schema=@databaseName;";

        private const string SqliteTbSysinfoCreate =
@"CREATE TABLE ""tb_sys_info"" (
""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
""Name"" TEXT UNIQUE,
""Value"" TEXT
);
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('Version', '1');
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('DatabaseName', @databaseName);";

        private const string SqliteTbSysinfoUpdateVersion = @"UPDATE ""tb_sys_info"" SET ""Value"" = @Value WHERE ""Name"" = 'Version';";

        private const string SqliteTbSysinfoRetrieve = @"SELECT * FROM ""tb_sys_info"";";

        private const string SqliteIsTableExistsStatement = "SELECT count(1) FROM sqlite_master where type='table' and name=@tableName;";
    }
}