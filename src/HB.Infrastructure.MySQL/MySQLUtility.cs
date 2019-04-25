using HB.Framework.Common;
using HB.Framework.Common.Entity;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace HB.Infrastructure.MySQL
{
    class DbTypeInfo
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DatabaseType { get; set; }
        /// <summary>
        /// 表达
        /// </summary>
        public string Statement { get; set; }
        /// <summary>
        /// 这个类型的值是否需要引号化
        /// </summary>
        public bool IsValueQuoted { get; set; }
    }

    static class MySQLUtility
    {
        //参数化
        public const string ParameterizedChar = "@";
        //引号化
        public const string QuotedChar = "'";
        //保留化
        public const string ReservedChar = "`";

        /// <summary>
        /// 类型与数据库类型映射字典
        /// </summary>
        private static readonly Dictionary<Type, DbTypeInfo> dbTypeInfoMap = InitDbTypeInfoMap();

        private static Dictionary<Type, DbTypeInfo> InitDbTypeInfoMap()
        {
            Dictionary<Type, DbTypeInfo> map = new Dictionary<Type, DbTypeInfo>
            {
                [typeof(byte)] = new DbTypeInfo() { DatabaseType = DbType.Byte, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(sbyte)] = new DbTypeInfo() { DatabaseType = DbType.SByte, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(short)] = new DbTypeInfo() { DatabaseType = DbType.Int16, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(ushort)] = new DbTypeInfo() { DatabaseType = DbType.UInt16, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(int)] = new DbTypeInfo() { DatabaseType = DbType.Int32, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(uint)] = new DbTypeInfo() { DatabaseType = DbType.UInt32, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(long)] = new DbTypeInfo() { DatabaseType = DbType.Int64, Statement = "BIGINT", IsValueQuoted = false },
                [typeof(ulong)] = new DbTypeInfo() { DatabaseType = DbType.UInt64, Statement = "BIGINT", IsValueQuoted = false },
                [typeof(float)] = new DbTypeInfo() { DatabaseType = DbType.Single, Statement = "DOUBLE", IsValueQuoted = false },
                [typeof(double)] = new DbTypeInfo() { DatabaseType = DbType.Double, Statement = "DOUBLE", IsValueQuoted = false },
                [typeof(decimal)] = new DbTypeInfo() { DatabaseType = DbType.Decimal, Statement = "DECIMAL", IsValueQuoted = false },
                [typeof(bool)] = new DbTypeInfo() { DatabaseType = DbType.Boolean, Statement = "BOOL", IsValueQuoted = false },
                [typeof(string)] = new DbTypeInfo() { DatabaseType = DbType.String, Statement = "VARCHAR", IsValueQuoted = true },
                [typeof(char)] = new DbTypeInfo() { DatabaseType = DbType.StringFixedLength, Statement = "CHAR", IsValueQuoted = true },
                [typeof(Guid)] = new DbTypeInfo() { DatabaseType = DbType.Guid, Statement = "CHAR(36)", IsValueQuoted = true },
                [typeof(DateTime)] = new DbTypeInfo() { DatabaseType = DbType.DateTime, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(DateTimeOffset)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(TimeSpan)] = new DbTypeInfo() { DatabaseType = DbType.Time, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(byte[])] = new DbTypeInfo() { DatabaseType = DbType.Binary, Statement = "BLOB", IsValueQuoted = false },
                [typeof(byte?)] = new DbTypeInfo() { DatabaseType = DbType.Byte, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(sbyte?)] = new DbTypeInfo() { DatabaseType = DbType.SByte, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(short?)] = new DbTypeInfo() { DatabaseType = DbType.Int16, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(ushort?)] = new DbTypeInfo() { DatabaseType = DbType.UInt16, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(int?)] = new DbTypeInfo() { DatabaseType = DbType.Int32, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(uint?)] = new DbTypeInfo() { DatabaseType = DbType.UInt32, Statement = "INTEGER", IsValueQuoted = false },
                [typeof(long?)] = new DbTypeInfo() { DatabaseType = DbType.Int64, Statement = "BIGINT", IsValueQuoted = false },
                [typeof(ulong?)] = new DbTypeInfo() { DatabaseType = DbType.UInt64, Statement = "BIGINT", IsValueQuoted = false },
                [typeof(float?)] = new DbTypeInfo() { DatabaseType = DbType.Single, Statement = "DOUBLE", IsValueQuoted = false },
                [typeof(double?)] = new DbTypeInfo() { DatabaseType = DbType.Double, Statement = "DOUBLE", IsValueQuoted = false },
                [typeof(decimal?)] = new DbTypeInfo() { DatabaseType = DbType.Decimal, Statement = "DECIMAL", IsValueQuoted = false },
                [typeof(bool?)] = new DbTypeInfo() { DatabaseType = DbType.Boolean, Statement = "BOOLEAN", IsValueQuoted = false },
                [typeof(char?)] = new DbTypeInfo() { DatabaseType = DbType.StringFixedLength, Statement = "CHAR", IsValueQuoted = true },
                [typeof(Guid?)] = new DbTypeInfo() { DatabaseType = DbType.Guid, Statement = "CHAR(36)", IsValueQuoted = true },
                [typeof(DateTime?)] = new DbTypeInfo() { DatabaseType = DbType.DateTime, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(DateTimeOffset?)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(TimeSpan?)] = new DbTypeInfo() { DatabaseType = DbType.Time, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(Object)] = new DbTypeInfo() { DatabaseType = DbType.Object, Statement = "", IsValueQuoted = true },
                [typeof(DBNull)] = new DbTypeInfo() { DatabaseType = DbType.Object, Statement = "", IsValueQuoted = false }
            };

            return map;
        }

        public static bool IsValueNeedQuoted(Type type)
        {
            if (type.IsEnum)
            {
                return false;
            }

            if (dbTypeInfoMap.ContainsKey(type))
            {
                return dbTypeInfoMap[type].IsValueQuoted;
            }

            return false;
        }

        public static DbType GetDbType(Type type)
        {
            if (type.IsEnum)
            {
                return DbType.String;
            }

            if (type.IsAssignableFrom(typeof(IList<string>)))
            {
                return dbTypeInfoMap[typeof(string)].DatabaseType;
            }

            if (type.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return dbTypeInfoMap[typeof(string)].DatabaseType;
            }

            return dbTypeInfoMap[type].DatabaseType;
        }

        public static string GetDbTypeStatement(Type type)
        {
            if (type.IsEnum)
            {
                return dbTypeInfoMap[typeof(string)].Statement;
            }

            if (type.IsAssignableFrom(typeof(IList<string>)))
            {
                return dbTypeInfoMap[typeof(string)].Statement;
            }

            if (type.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return dbTypeInfoMap[typeof(string)].Statement;
            }

            return dbTypeInfoMap[type].Statement;
        }

        public static string GetQuoted(string name)
        {
            return QuotedChar + name.Replace(QuotedChar, QuotedChar + QuotedChar) + QuotedChar;
        }

        public static string GetParameterized(string name)
        {
            return ParameterizedChar + name;
        }

        public static string GetReserved(string name)
        {
            return ReservedChar + name + ReservedChar;
        }

        /// <summary>
        /// 获取类型值对应的数据库值的表达
        /// 包含安全考虑
        /// </summary>
        /// <param name="value">类型值</param>
        /// <returns>数据库类型值的表达</returns>
        public static string GetDbValueStatement(object value, bool needQuoted)
        {
            string valueStr = ValueConverter.TypeValueToDbValue(value);

            valueStr = SafeDbStatement(valueStr);

            if (valueStr == null)
            {
                return null;
            }

            if (needQuoted && MySQLUtility.IsValueNeedQuoted(value.GetType()))
            {
                valueStr = GetQuoted(valueStr);
            }

            return valueStr;
        }

        /// <summary>
        /// 将数据库值表达，进行安全过滤
        /// </summary>
        /// <param name="dbValue"></param>
        /// <returns></returns>
        public static string SafeDbStatement(string dbValueStatement)
        {
            if (string.IsNullOrEmpty(dbValueStatement))
            {
                return dbValueStatement;
            }
            //TODO:增加对值的过滤，预防SQL注入
            return dbValueStatement.ToString(GlobalSettings.Culture)
                .Replace("'", "''")
                .Replace("--", "")
                .Replace("/*", "")
                .Replace("*/", "");
        }
    }
}
