#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Def;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    internal static class SqlHelper
    {
        public static string CreateAddSql(EntityDef entityDef, DatabaseEngineType engineType, bool returnId, int number = 0)
        {
            StringBuilder args = new StringBuilder();
            StringBuilder values = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                if (propertyDef.IsAutoIncrementPrimaryKey)
                {
                    continue;
                }

                args.Append($"{propertyDef.DbReservedName},");

                values.Append($"{propertyDef.DbParameterizedName}_{number},");
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId ? $"select {GetLastInsertIdStatement(engineType)};" : string.Empty;

            return $"insert into {entityDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        public static string CreateUpdateSql(EntityDef entityDef, int number = 0)
        {
            StringBuilder args = new StringBuilder();

            foreach (EntityPropertyDef propertyInfo in entityDef.PropertyDefs)
            {
                if (propertyInfo.IsAutoIncrementPrimaryKey || propertyInfo.Name == nameof(Entity.Guid))
                {
                    continue;
                }

                args.Append($" {propertyInfo.DbReservedName}={propertyInfo.DbParameterizedName}_{number},");
            }

            args.RemoveLast();

            EntityPropertyDef idProperty = entityDef.GetPropertyDef(nameof(Entity.Id))!;
            EntityPropertyDef deletedProperty = entityDef.GetPropertyDef(nameof(Entity.Deleted))!;
            EntityPropertyDef versionProperty = entityDef.GetPropertyDef(nameof(Entity.Version))!;

            StringBuilder where = new StringBuilder();

            where.Append($"{idProperty.DbReservedName}={idProperty.DbParameterizedName}_{number} AND ");
            where.Append($"{versionProperty.DbReservedName}={versionProperty.DbParameterizedName}_{number} - 1 AND ");
            where.Append($"{deletedProperty.DbReservedName}=0");

            return $"UPDATE {entityDef.DbTableReservedName} SET {args} WHERE {where};";
        }

        public static string CreateDeleteSql(EntityDef entityDef, int number = 0)
        {
            return CreateUpdateSql(entityDef, number);
        }

        public static string CreateSelectSql(params EntityDef[] entityDefs)
        {
            StringBuilder builder = new StringBuilder("SELECT ");

            foreach (EntityDef entityDef in entityDefs)
            {
                string DbTableReservedName = entityDef.DbTableReservedName;

                foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
                {
                    builder.Append($"{DbTableReservedName}.{propertyDef.DbReservedName},");
                }
            }

            builder.RemoveLast();

            return builder.ToString();
        }

        /// <summary>
        /// 用于专有化的字符（`）
        /// </summary>
        public static string GetReservedChar(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => "`",
                DatabaseEngineType.SQLite => @"""",
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupported)
            };
        }

        /// <summary>
        /// 用于参数化的字符（@）,用于参数化查询
        /// </summary>
        public const string ParameterizedChar = "@";

        /// <summary>
        /// 用于引号化的字符(')，用于字符串
        /// </summary>
        public const string QuotedChar = "'";

        public static string GetQuoted(string name)
        {
            return QuotedChar + name.Replace(QuotedChar, QuotedChar + QuotedChar, GlobalSettings.Comparison) + QuotedChar;
        }

        public static string GetParameterized(string name)
        {
            return ParameterizedChar + name;
        }

        public static string GetReserved(string name, DatabaseEngineType engineType)
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

        public static bool IsDbFieldNeedLength(EntityPropertyDef propertyDef, DatabaseEngineType engineType)
        {
            DbType dbType = TypeConvert.TypeToDbType(propertyDef, engineType);

            return dbType == DbType.String
                || dbType == DbType.StringFixedLength
                || dbType == DbType.AnsiString
                || dbType == DbType.AnsiStringFixedLength
                || dbType == DbType.VarNumeric;
        }

        public static string TempTable_Insert_Id(string tempTableName, string value, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                DatabaseEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupported)
            };
        }

        public static string TempTable_Select_Id(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"select `id` from `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Drop(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        public static string TempTable_Create_Id(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                DatabaseEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        public static string FoundChanges_Statement(DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回的查询语句的结果
                //row_count表示修改时找到的条数
                //默认下UserAffectRows=false，两者相同。当UserAffectedRow=true时，row_count()只会返回真正修改的行数，找到但值相同没有修改的不算
                DatabaseEngineType.MySQL => $"row_count()", // $" found_rows() ",
                DatabaseEngineType.SQLite => $" changes() ",
                _ => "",
            };
        }

        public static string GetLastInsertIdStatement(DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.SQLite => "last_insert_rowid()",
                DatabaseEngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        public static string GetOrderBySqlUtilInStatement(string quotedColName, string[] ins, DatabaseEngineType databaseEngineType)
        {
            if (databaseEngineType == DatabaseEngineType.MySQL)
            {
                return $" ORDER BY FIELD({quotedColName}, {ins.ToJoinedString(",")}) ";
            }
            else if (databaseEngineType == DatabaseEngineType.SQLite)
            {
                StringBuilder orderCaseBuilder = new StringBuilder(" ORDER BY CASE ");

                orderCaseBuilder.Append(quotedColName);

                for (int i = 0; i < ins.Length; ++i)
                {
                    orderCaseBuilder.Append($" when {ins[i]} THEN {i} ");
                }

                orderCaseBuilder.Append(" END ");

                return orderCaseBuilder.ToString();
            }

            throw new DatabaseException(ErrorCode.DatabaseUnSupported);
        }

        public static string SQLite_Table_Create_Statement(EntityDef entityDef, bool addDropStatement)
        {
            StringBuilder propertyInfoSql = new StringBuilder();

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, DatabaseEngineType.SQLite);

                string nullable = propertyDef.IsNullable ? "" : " NOT NULL ";

                string unique = propertyDef.IsUnique && !propertyDef.IsForeignKey && !propertyDef.IsAutoIncrementPrimaryKey ? " UNIQUE " : "";

                string primaryStatement = propertyDef.Name == "Id" ? " PRIMARY KEY AUTOINCREMENT " : "";

                propertyInfoSql.Append($" {propertyDef.DbReservedName} {dbTypeStatement} {primaryStatement} {nullable} {unique} ,");
            }

            propertyInfoSql.Remove(propertyInfoSql.Length - 1, 1);

            string dropStatement = addDropStatement ? $"Drop table if exists {entityDef.DbTableReservedName};" : string.Empty;

            string tableCreateSql = $"{dropStatement} CREATE TABLE {entityDef.DbTableReservedName} ({propertyInfoSql});";

            return tableCreateSql;
        }

        public static string MySQL_Table_Create_Statement(EntityDef entityDef, bool addDropStatement, int varcharDefaultLength)
        {
            StringBuilder propertySqlBuilder = new StringBuilder();

            if (entityDef.DbTableReservedName.IsNullOrEmpty())
            {
                throw new DatabaseException($"Type : {entityDef.EntityFullName} has null or empty DbTableReservedName");
            }

            foreach (EntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, DatabaseEngineType.MySQL);

                int length = 0;

                if (IsDbFieldNeedLength(propertyDef, DatabaseEngineType.MySQL) && !dbTypeStatement.Contains('(', StringComparison.InvariantCulture))
                {
                    if ((propertyDef.DbMaxLength == null || propertyDef.DbMaxLength == 0))
                    {
                        length = varcharDefaultLength;
                    }
                    else
                    {
                        length = propertyDef.DbMaxLength.Value;
                    }
                }

                if (length >= 16383) //因为utf8mb4编码，一个汉字4个字节
                {
                    dbTypeStatement = "MEDIUMTEXT";
                }

                if (length >= 4194303)
                {
                    throw new DatabaseException($"字段长度太长。{propertyDef.EntityDef.EntityFullName} : {propertyDef.Name}");
                }

                if (propertyDef.IsLengthFixed)
                {
                    dbTypeStatement = "CHAR";
                }

                string lengthStatement = (length == 0 || dbTypeStatement == "MEDIUMTEXT") ? "" : "(" + length + ")";
                string nullableStatement = propertyDef.IsNullable == true ? "" : " NOT NULL ";
                string autoIncrementStatement = propertyDef.Name == "Id" ? "AUTO_INCREMENT" : "";
                string uniqueStatement = !propertyDef.IsAutoIncrementPrimaryKey && !propertyDef.IsForeignKey && propertyDef.IsUnique ? " UNIQUE " : "";

                propertySqlBuilder.Append($" {propertyDef.DbReservedName} {dbTypeStatement}{lengthStatement} {nullableStatement} {autoIncrementStatement} {uniqueStatement},");
            }

            string dropStatement = addDropStatement ? $"Drop table if exists {entityDef.DbTableReservedName};" : string.Empty;

            return $"{dropStatement} create table {entityDef.DbTableReservedName} ( {propertySqlBuilder} PRIMARY KEY (`Id`)) ENGINE=InnoDB   DEFAULT CHARSET=utf8mb4;";
        }

        public static string GetTableCreateSql(EntityDef entityDef, bool addDropStatement, int varcharDefaultLength, DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => MySQL_Table_Create_Statement(entityDef, addDropStatement, varcharDefaultLength),
                DatabaseEngineType.SQLite => SQLite_Table_Create_Statement(entityDef, addDropStatement),
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupported)
            };
        }

        public static string GetIsTableExistSql(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => _mysql_isTableExistsStatement,
                DatabaseEngineType.SQLite => _sqlite_isTableExistsStatement,
                _ => throw new NotImplementedException()
            };
        }

        public static string GetSystemInfoRetrieveSql(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => _mysql_tbSysInfoRetrieve,
                DatabaseEngineType.SQLite => _sqlite_tbSysInfoRetrieve,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoUpdateVersionSql(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => _mysql_tbSysInfoUpdateVersion,
                DatabaseEngineType.SQLite => _sqlite_tbSysInfoUpdateVersion,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoCreateSql(DatabaseEngineType engineType)
        {
            return engineType switch
            {
                DatabaseEngineType.MySQL => _mysql_tbSysInfoCreate,
                DatabaseEngineType.SQLite => _sqlite_tbSysInfoCreate,
                _ => string.Empty
            };
        }

        private const string _mysql_tbSysInfoCreate =
        @"CREATE TABLE `tb_sys_info` (
	`Id` int (11) NOT NULL AUTO_INCREMENT,
	`Name` varchar(100) DEFAULT NULL,
	`Value` varchar(1024) DEFAULT NULL,
	PRIMARY KEY(`Id`),
	UNIQUE KEY `Name_UNIQUE` (`Name`)
);
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('Version', '1');
INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('DatabaseName', @databaseName);";

        private const string _mysql_tbSysInfoUpdateVersion = @"UPDATE `tb_sys_info` SET `Value` = @Value WHERE `Name` = 'Version';";

        private const string _mysql_tbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private const string _mysql_isTableExistsStatement = "SELECT count(1) FROM information_schema.TABLES WHERE table_name =@tableName and table_schema=@databaseName;";

        private const string _sqlite_tbSysInfoCreate =
        @"CREATE TABLE ""tb_sys_info"" (
	""Id"" INTEGER PRIMARY KEY AUTOINCREMENT,
	""Name"" TEXT UNIQUE,
	""Value"" TEXT
);
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('Version', '1');
INSERT INTO ""tb_sys_info""(""Name"", ""Value"") VALUES('DatabaseName', @databaseName);";

        private const string _sqlite_tbSysInfoUpdateVersion = @"UPDATE ""tb_sys_info"" SET ""Value"" = @Value WHERE ""Name"" = 'Version';";

        private const string _sqlite_tbSysInfoRetrieve = @"SELECT * FROM ""tb_sys_info"";";

        private const string _sqlite_isTableExistsStatement = "SELECT count(1) FROM sqlite_master where type='table' and name=@tableName;";
    }
}