/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Data;
using System.Text;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

using HB.FullStack.Database.Implements;

namespace HB.FullStack.Database.SQL
{
    internal static partial class SqlHelper
    {
        #region Schema Create

        public static bool IsDbFieldNeedLength(DbModelPropertyDef propertyDef, DbEngineType engineType)
        {
            DbType dbType = DbPropertyConvert.PropertyTypeToDbType(propertyDef, engineType);

            return dbType == DbType.String
                || dbType == DbType.StringFixedLength
                || dbType == DbType.AnsiString
                || dbType == DbType.AnsiStringFixedLength
                || dbType == DbType.VarNumeric;
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

                propertyInfoSql.Append($" {propertyDef.DbReservedName} {dbTypeStatement} {primaryStatement} {nullable} {unique} ,");

                //索引
                if (!propertyDef.IsUnique && !propertyDef.IsAutoIncrementPrimaryKey && (propertyDef.IsForeignKey || propertyDef.IsIndexNeeded))
                {
                    indexSqlBuilder.Append($" create index {modelDef.TableName}_{propertyDef.Name}_index on {modelDef.DbTableReservedName} ({propertyDef.DbReservedName}); ");
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
                    throw DbExceptions.ModelError(propertyDef.ModelDef.FullName, propertyDef.Name, "字段长度太长");
                }

                //if (propertyDef.IsLengthFixed )
                //{
                //	dbTypeStatement = "CHAR";
                //}

                string lengthStatement = (length == 0 || dbTypeStatement == "MEDIUMTEXT") ? "" : "(" + length + ")";
                string nullableStatement = propertyDef.IsNullable == true ? "" : " NOT NULL ";
                string autoIncrementStatement = propertyDef.IsAutoIncrementPrimaryKey ? "AUTO_INCREMENT" : "";
                string uniqueStatement = !propertyDef.IsPrimaryKey && !propertyDef.IsForeignKey && propertyDef.IsUnique ? " UNIQUE " : "";

                propertySqlBuilder.Append($" {propertyDef.DbReservedName} {dbTypeStatement}{lengthStatement} {nullableStatement} {autoIncrementStatement} {uniqueStatement},");

                //判断索引
                if (propertyDef.IsForeignKey || propertyDef.IsIndexNeeded)
                {
                    indexSqlBuilder.Append($" INDEX {propertyDef.Name}_index ({propertyDef.DbReservedName}), ");
                }
            }

            if (primaryKeyPropertyDef == null)
            {
                throw DbExceptions.ModelError(modelDef.FullName, "", "no primary key");
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

        private const string MySqlTbSysinfoCreate = $"""
            CREATE TABLE `tb_sys_info` (
                `Id` int (11) NOT NULL AUTO_INCREMENT,
                `Name` varchar(100) DEFAULT NULL,
                `Value` varchar(1024) DEFAULT NULL,
                PRIMARY KEY(`Id`),
                UNIQUE KEY `Name_UNIQUE` (`Name`)
            );
            INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('{SystemInfoNames.VERSION}', '1');
            INSERT INTO `tb_sys_info`(`Name`, `Value`) VALUES('{SystemInfoNames.DATABASE_SCHEMA}', @{SystemInfoNames.DATABASE_SCHEMA});
            """;

        private const string MySqlTbSysInfoUpdateVersion = $"""
            UPDATE `tb_sys_info` SET `Value` = @Value WHERE `Name` = '{SystemInfoNames.VERSION}';
            """;

        private const string MySqlTbSysInfoRetrieve = @"SELECT * FROM `tb_sys_info`;";

        private const string MySqlIsTableExistsStatement = """
            SELECT count(1) FROM information_schema.TABLES WHERE table_name =@tableName and table_schema=Database();
            """;

        private const string SqliteTbSysinfoCreate = $"""
            CREATE TABLE ""tb_sys_info"" (
                "Id" INTEGER PRIMARY KEY AUTOINCREMENT,
                "Name" TEXT UNIQUE,
                "Value" TEXT
            );
            INSERT INTO "tb_sys_info"("Name", "Value") VALUES('{SystemInfoNames.VERSION}', '1');
            INSERT INTO "tb_sys_info"("Name", "Value") VALUES('{SystemInfoNames.DATABASE_SCHEMA}', @{SystemInfoNames.DATABASE_SCHEMA});
            """;

        private const string SqliteTbSysinfoUpdateVersion = $@"UPDATE ""tb_sys_info"" SET ""Value"" = @Value WHERE ""Name"" = '{SystemInfoNames.VERSION}';";

        private const string SqliteTbSysinfoRetrieve = @"SELECT * FROM ""tb_sys_info"";";

        private const string SqliteIsTableExistsStatement = "SELECT count(1) FROM sqlite_master where type='table' and name=@tableName;";

        #endregion
    }
}