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

                args.Append($"{propertyDef.DbReservedName},");

                values.Append($"{propertyDef.DbParameterizedName}_{number},");
            }

            args.RemoveLast();
            values.RemoveLast();

            string returnIdStatement = returnId ? $"select {GetLastInsertIdStatement(engineType)};" : string.Empty;

            return $"insert into {entityDef.DbTableReservedName}({args}) values({values});{returnIdStatement}";
        }

        public static string CreateUpdateEntitySql(EntityDef entityDef, int number = 0)
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
                    builder.Append($"{DbTableReservedName}.{propertyDef.DbReservedName},");
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
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupported)
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

        public static string FoundChanges_Statement(EngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                //found_rows()返回的查询语句的结果
                //row_count表示修改时找到的条数
                //默认下UserAffectRows=false，两者相同。当UserAffectedRow=true时，row_count()只会返回真正修改的行数，找到但值相同没有修改的不算
                EngineType.MySQL => $"row_count()", // $" found_rows() ",
                EngineType.SQLite => $" changes() ",
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
                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, EngineType.SQLite);

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
                string dbTypeStatement = TypeConvert.TypeToDbTypeStatement(propertyDef, EngineType.MySQL);

                int length = 0;

                if (IsDbFieldNeedLength(propertyDef, EngineType.MySQL) && !dbTypeStatement.Contains('(', StringComparison.InvariantCulture))
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

        public static string GetTableCreateSql(EntityDef entityDef, bool addDropStatement, int varcharDefaultLength, EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => MySQL_Table_Create_Statement(entityDef, addDropStatement, varcharDefaultLength),
                EngineType.SQLite => SQLite_Table_Create_Statement(entityDef, addDropStatement),
                _ => throw new DatabaseException(ErrorCode.DatabaseUnSupported)
            };
        }

        public static string GetIsTableExistSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysql_isTableExistsStatement,
                EngineType.SQLite => _sqlite_isTableExistsStatement,
                _ => throw new NotImplementedException()
            };
        }

        public static string GetSystemInfoRetrieveSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysql_tbSysInfoRetrieve,
                EngineType.SQLite => _sqlite_tbSysInfoRetrieve,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoUpdateVersionSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysql_tbSysInfoUpdateVersion,
                EngineType.SQLite => _sqlite_tbSysInfoUpdateVersion,
                _ => string.Empty
            };
        }

        public static string GetSystemInfoCreateSql(EngineType engineType)
        {
            return engineType switch
            {
                EngineType.MySQL => _mysql_tbSysInfoCreate,
                EngineType.SQLite => _sqlite_tbSysInfoCreate,
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