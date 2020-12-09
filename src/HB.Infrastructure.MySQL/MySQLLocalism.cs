#nullable enable
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace HB.Infrastructure.MySQL
{
    internal class DbTypeInfo
    {
        /// <summary>
        /// 数据库类型
        /// </summary>
        public DbType DatabaseType { get; set; }

        /// <summary>
        /// DbType在不同数据库中的表达，用来创建表的时候用
        /// </summary>
        public string Statement { get; set; } = default!;

        /// <summary>
        /// 这个类型的值是否需要引号化
        /// </summary>
        public bool IsValueQuoted { get; set; }

    }

    internal static class MySQLLocalism
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
        private static readonly Dictionary<Type, DbTypeInfo> _dbTypeInfoMap = InitDbTypeInfoMap();

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
                //[typeof(DateTime)] = new DbTypeInfo() { DatabaseType = DbType.DateTime, Statement = "DATETIME", IsValueQuoted = true },
                //[typeof(DateTimeOffset)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(DateTimeOffset)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = false },
                //[typeof(TimeSpan)] = new DbTypeInfo() { DatabaseType = DbType.Time, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(TimeSpan)] = new DbTypeInfo() { DatabaseType = DbType.Int64, Statement = "BIGINT", IsValueQuoted = false },
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
                //[typeof(DateTime?)] = new DbTypeInfo() { DatabaseType = DbType.DateTime, Statement = "DATETIME", IsValueQuoted = true },
                //[typeof(DateTimeOffset?)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(DateTimeOffset?)] = new DbTypeInfo() { DatabaseType = DbType.DateTimeOffset, Statement = "DATETIME", IsValueQuoted = false },
                //[typeof(TimeSpan?)] = new DbTypeInfo() { DatabaseType = DbType.Time, Statement = "DATETIME", IsValueQuoted = true },
                [typeof(TimeSpan?)] = new DbTypeInfo() { DatabaseType = DbType.Int64, Statement = "BIGINT", IsValueQuoted = false },
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

            if (_dbTypeInfoMap.ContainsKey(type))
            {
                return _dbTypeInfoMap[type].IsValueQuoted;
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
                return _dbTypeInfoMap[typeof(string)].DatabaseType;
            }

            if (type.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return _dbTypeInfoMap[typeof(string)].DatabaseType;
            }

            return _dbTypeInfoMap[type].DatabaseType;
        }

        public static string GetDbTypeStatement(Type type)
        {
            if (type.IsEnum)
            {
                return _dbTypeInfoMap[typeof(string)].Statement;
            }

            if (type.IsAssignableFrom(typeof(IList<string>)))
            {
                return _dbTypeInfoMap[typeof(string)].Statement;
            }

            if (type.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return _dbTypeInfoMap[typeof(string)].Statement;
            }

            return _dbTypeInfoMap[type].Statement;
        }

        public static string GetQuoted(string name)
        {
            return QuotedChar + name.Replace(QuotedChar, QuotedChar + QuotedChar, GlobalSettings.Comparison) + QuotedChar;
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
        [return: NotNullIfNotNull("value")]
        public static string? GetDbValueStatement(object? value, bool needQuoted)
        {
            if (value == null)
            {
                return null;
            }

            string valueStr = ValueConverterUtil.TypeValueToStringValue(value)!;

            if (needQuoted && IsValueNeedQuoted(value.GetType()))
            {
                valueStr = GetQuoted(valueStr);
            }

            return valueStr;
        }
    }
}