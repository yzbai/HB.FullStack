using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

namespace HB.FullStack.Database.SQL
{
    internal partial class SQLBuilder
    {
        private static string TempTable_Insert_Id(string tempTableName, string value, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"insert into `{tempTableName}`(`id`) values({value});",
                DatabaseEngineType.SQLite => $"insert into temp.{tempTableName}(\"id\") values({value});",
                _ => "",
            };
        }

        private static string TempTable_Select_Id(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"select `id` from `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"select id from temp.{tempTableName};",
                _ => "",
            };
        }

        private static string TempTable_Drop(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"drop temporary table if exists `{tempTableName}`;",
                DatabaseEngineType.SQLite => $"drop table if EXISTS temp.{tempTableName};",
                _ => "",
            };
        }

        private static string TempTable_Create_Id(string tempTableName, DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.MySQL => $"create temporary table `{tempTableName}` ( `id` int not null);",
                DatabaseEngineType.SQLite => $"create temporary table temp.{tempTableName} (\"id\" integer not null);",
                _ => "",
            };
        }

        private static string FoundChanges_Statement(DatabaseEngineType databaseEngineType)
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

        private static string GetLastInsertIdStatement(DatabaseEngineType databaseEngineType)
        {
            return databaseEngineType switch
            {
                DatabaseEngineType.SQLite => "last_insert_rowid()",
                DatabaseEngineType.MySQL => "last_insert_id()",
                _ => "",
            };
        }

        private static string SQLite_Table_Create_Statement(DatabaseEntityDef entityDef, bool addDropStatement, IDatabaseEngine engine)
        {
            StringBuilder propertyInfoSql = new StringBuilder();

            foreach (DatabaseEntityPropertyDef info in entityDef.PropertyDefs)
            {
                string dbTypeStatement = info.TypeConverter == null ? engine.GetDbTypeStatement(info.Type) : info.TypeConverter.TypeToDbTypeStatement(info.Type);

                string nullable = info.IsNullable ? "" : " NOT NULL ";

                string unique = info.IsUnique && !info.IsForeignKey && !info.IsAutoIncrementPrimaryKey ? " UNIQUE " : "";

                string primaryStatement = info.Name == "Id" ? " PRIMARY KEY AUTOINCREMENT " : "";

                propertyInfoSql.Append($" {info.DbReservedName} {dbTypeStatement} {primaryStatement} {nullable} {unique} ,");
            }

            propertyInfoSql.Remove(propertyInfoSql.Length - 1, 1);

            string dropStatement = addDropStatement ? $"Drop table if exists {entityDef.DbTableReservedName};" : string.Empty;

            string tableCreateSql = $"{dropStatement} CREATE TABLE {entityDef.DbTableReservedName} ({propertyInfoSql});";

            return tableCreateSql;
        }



        private static string MySQL_Table_Create_Statement(DatabaseEntityDef entityDef, bool addDropStatement, IDatabaseEngine databaseEngine, int varcharDefaultLength)
        {
            StringBuilder propertySqlBuilder = new StringBuilder();

            if (entityDef.DbTableReservedName.IsNullOrEmpty())
            {
                throw new DatabaseException($"Type : {entityDef.EntityFullName} has null or empty DbTableReservedName");
            }

            foreach (DatabaseEntityPropertyDef propertyDef in entityDef.PropertyDefs)
            {
                string dbTypeStatement = propertyDef.TypeConverter == null
                    ? databaseEngine.GetDbTypeStatement(propertyDef.Type)
                    : propertyDef.TypeConverter.TypeToDbTypeStatement(propertyDef.Type);

                int length = 0;

                if (NeedDbFieldLength(propertyDef.Type))
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

            static bool NeedDbFieldLength(Type type)
            {
                return type == typeof(string) || type == typeof(char) || type == typeof(char?);
            }
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
