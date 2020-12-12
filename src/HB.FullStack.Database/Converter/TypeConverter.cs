#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;
using HB.FullStack.Database.SQL;

namespace System
{
    internal static class TypeConverter
    {
        private static readonly Dictionary<Type, DbType> _typeToDbTypeDict = new Dictionary<Type, DbType>
        {
            [typeof(byte)] = DbType.Byte,
            [typeof(sbyte)] = DbType.SByte,
            [typeof(short)] = DbType.Int16,
            [typeof(ushort)] = DbType.UInt16,
            [typeof(int)] = DbType.Int32,
            [typeof(uint)] = DbType.UInt32,
            [typeof(long)] = DbType.Int64,
            [typeof(ulong)] = DbType.UInt64,
            [typeof(float)] = DbType.Single,
            [typeof(double)] = DbType.Double,
            [typeof(decimal)] = DbType.Decimal,
            [typeof(bool)] = DbType.Boolean,
            [typeof(string)] = DbType.String,
            [typeof(char)] = DbType.StringFixedLength,
            [typeof(Guid)] = DbType.Guid,
            [typeof(DateTimeOffset)] = DbType.DateTimeOffset,
            [typeof(byte[])] = DbType.Binary
        };

        private static readonly Dictionary<Type, string> _mysqlTypeToDbTypeStatementDict = new Dictionary<Type, string>
        {
            [typeof(byte)] = "TINYINT",
            [typeof(sbyte)] = "TINYINT",
            [typeof(short)] = "SMALLINT",
            [typeof(ushort)] = "SMALLINT",
            [typeof(int)] = "INT",
            [typeof(uint)] = "INT",
            [typeof(long)] = "BIGINT",
            [typeof(ulong)] = "BIGINT",
            [typeof(float)] = "FLOAT",
            [typeof(double)] = "DOUBLE",
            [typeof(decimal)] = "DECIMAL",
            [typeof(bool)] = "TINYINT",
            [typeof(string)] = "VARCHAR",
            [typeof(char)] = "CHAR",
            [typeof(Guid)] = "CHAR(36)",
            [typeof(DateTimeOffset)] = "DATETIME(6)",
            [typeof(byte[])] = "Binary"
        };

        private static readonly Dictionary<Type, string> _sqliteTypeToDbTypeStatementDict = new Dictionary<Type, string>
        {
            [typeof(byte)] = "INTEGER",
            [typeof(sbyte)] = "INTEGER",
            [typeof(short)] = "INTEGER",
            [typeof(ushort)] = "INTEGER",
            [typeof(int)] = "INTEGER",
            [typeof(uint)] = "INTEGER",
            [typeof(long)] = "INTEGER",
            [typeof(ulong)] = "INTEGER",
            [typeof(float)] = "DOUBLE",
            [typeof(double)] = "DOUBLE",
            [typeof(decimal)] = "DECIMAL",
            [typeof(bool)] = "BOOL",
            [typeof(string)] = "VARCHAR",
            [typeof(char)] = "CHAR",
            [typeof(Guid)] = "CHAR(36)",
            [typeof(DateTimeOffset)] = "VARCHAR",
            [typeof(byte[])] = "BLOB"
        };

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Entity的Type值. 逻辑同EntityMapperCreator一致
        /// </summary>
        public static object? DbValueToTypeValue(object dbValue, DatabaseEntityPropertyDef propertyDef) //Type targetType)
        {
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbValueToTypeValue(dbValue);
            }

            Type dbValueType = dbValue.GetType();

            if (dbValueType == typeof(DBNull))
            {
                return default;
            }

            Type targetType = propertyDef.Type;
            Type? underType = propertyDef.NullableUnderlyingType;

            object rt;

            if (targetType.IsEnum || (underType != null && underType.IsEnum))
            {
                if (underType == null)
                {
                    rt = Enum.Parse(targetType, dbValue.ToString(), true);
                }
                else
                {
                    rt = Enum.Parse(underType, dbValue.ToString(), true);
                }
            }
            else if (targetType == typeof(DateTimeOffset) || (underType != null && underType == typeof(DateTimeOffset)))
            {
                if (dbValueType == typeof(string))
                {
                    rt = DateTimeOffset.Parse(dbValue.ToString(), CultureInfo.InvariantCulture);
                }
                else
                {
                    rt = new DateTimeOffset((DateTime)dbValue, TimeSpan.Zero);
                }
            }
            else
            {
                if (underType == null && dbValueType != targetType)
                {
                    rt = Convert.ChangeType(dbValue, targetType, CultureInfo.InvariantCulture);
                }
                else if (underType != null && dbValueType != underType)
                {
                    rt = Convert.ChangeType(dbValue, underType, CultureInfo.InvariantCulture);
                }
                else
                {
                    rt = dbValue;
                }
            }

            if (underType == null)
            {
                return rt;
            }

            ConstructorInfo ctor = targetType.GetConstructor(new Type[] { underType });

            return ctor.Invoke(new object[] { rt });
        }

        public static object TypeValueToDbValue(object? typeValue, DatabaseEntityPropertyDef propertyDef)
        {
            if (propertyDef.TypeConverter != null)
            {
                string? dbvalue = propertyDef.TypeConverter.TypeValueToDbValue(typeValue);

                if (dbvalue == null)
                {
                    return DBNull.Value;
                }

                return dbvalue;
            }

            if (typeValue == null)
            {
                return DBNull.Value;
            }

            if (propertyDef.Type.IsEnum || (propertyDef.NullableUnderlyingType != null && propertyDef.NullableUnderlyingType.IsEnum))
            {
                return typeValue.ToString();
            }

            return typeValue;
        }

        public static string TypeValueToDbValueStatement(object? value, bool quotedIfNeed)
        {
            string result = value switch
            {
                null => "null",
                //Enum e => e.ToString(),
                DBNull _ => "null",
                DateTime _ => throw new DatabaseException(ErrorCode.UseDateTimeOffsetOnly),
                DateTimeOffset dt => dt.ToString(CultureInfo.InvariantCulture),
                bool b => b ? "1" : "0",
                _ => value.ToString()
            };

            if (!quotedIfNeed || result == "null" || !SqlHelper.IsValueNeedQuoted(value!.GetType()))
            {
                return result;
            }

            return SqlHelper.GetQuoted(result);
        }

        public static DbType TypeToDbType(DatabaseEntityPropertyDef propertyDef)
        {
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.TypeToDbType(propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            if (trueType.IsEnum)
            {
                return DbType.String;
            }

            if (_typeToDbTypeDict.TryGetValue(trueType, out DbType dbType))
            {
                return dbType;
            }

            throw new DatabaseException(ErrorCode.DatabaseUnSupported, $"Unspoorted Type:{propertyDef.NullableUnderlyingType ?? propertyDef.Type}, Property:{propertyDef.Name}, Entity:{propertyDef.EntityDef.EntityFullName}");
        }

        public static string TypeToDbTypeStatement(DatabaseEntityPropertyDef propertyDef, DatabaseEngineType engineType)
        {
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.TypeToDbTypeStatement(propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            if (trueType.IsEnum)
            {
                trueType = typeof(string);
            }

            return engineType switch
            {
                DatabaseEngineType.MySQL => _mysqlTypeToDbTypeStatementDict[trueType],
                DatabaseEngineType.SQLite => _sqliteTypeToDbTypeStatementDict[trueType],
                _ => throw new NotImplementedException(),
            };
        }
    }
}