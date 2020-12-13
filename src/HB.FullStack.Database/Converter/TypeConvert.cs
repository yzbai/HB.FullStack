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
    internal static class TypeConvert
    {
        private static readonly Dictionary<Type, TypeConverter> _mysqlGlobalConverters = new Dictionary<Type, TypeConverter>
        {
            [typeof(byte)] = new TypeConverter { Statement = "TINYINT", DbType = DbType.Byte },
            [typeof(sbyte)] = new TypeConverter { Statement = "TINYINT", DbType = DbType.SByte },
            [typeof(short)] = new TypeConverter { Statement = "SMALLINT", DbType = DbType.Int16 },
            [typeof(ushort)] = new TypeConverter { Statement = "SMALLINT", DbType = DbType.UInt16 },
            [typeof(int)] = new TypeConverter { Statement = "INT", DbType = DbType.Int32 },
            [typeof(uint)] = new TypeConverter { Statement = "INT", DbType = DbType.UInt32 },
            [typeof(long)] = new TypeConverter { Statement = "BIGINT", DbType = DbType.Int64 },
            [typeof(ulong)] = new TypeConverter { Statement = "BIGINT", DbType = DbType.UInt64 },
            [typeof(float)] = new TypeConverter { Statement = "FLOAT", DbType = DbType.Single },
            [typeof(double)] = new TypeConverter { Statement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new TypeConverter { Statement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new TypeConverter { Statement = "TINYINT", DbType = DbType.Boolean },
            [typeof(string)] = new TypeConverter { Statement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new TypeConverter { Statement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new TypeConverter { Statement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new TypeConverter { Statement = "DATETIME(6)", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new TypeConverter { Statement = "Binary", DbType = DbType.Binary }
        };

        private static readonly Dictionary<Type, TypeConverter> _sqliteGlobalConverters = new Dictionary<Type, TypeConverter>
        {
            [typeof(byte)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.Byte },
            [typeof(sbyte)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.SByte },
            [typeof(short)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.Int16 },
            [typeof(ushort)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.UInt16 },
            [typeof(int)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.Int32 },
            [typeof(uint)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.UInt32 },
            [typeof(long)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.Int64 },
            [typeof(ulong)] = new TypeConverter { Statement = "INTEGER", DbType = DbType.UInt64 },
            [typeof(float)] = new TypeConverter { Statement = "DOUBLE", DbType = DbType.Single },
            [typeof(double)] = new TypeConverter { Statement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new TypeConverter { Statement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new TypeConverter { Statement = "BOOL", DbType = DbType.Boolean },
            [typeof(string)] = new TypeConverter { Statement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new TypeConverter { Statement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new TypeConverter { Statement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new TypeConverter { Statement = "VARCHAR", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new TypeConverter { Statement = "BLOB", DbType = DbType.Binary }
        };

        private static readonly TypeConverter _mysqlDataTimeOffsetConverter = new TypeConverter
        {
            DbType = DbType.Int64,
            Statement = "BIGINT",

            TypeValueToDbValue = (typeValue, propertyType) =>
            {
                if (typeValue == null)
                {
                    return DBNull.Value;
                }

                return ((DateTimeOffset)typeValue).Ticks;
            },

            DbValueToTypeValue = (dbValue, propertyType) =>
            {
                Type dbValueType = dbValue.GetType();

                if (dbValueType == typeof(DBNull))
                {
                    return default(DateTimeOffset);
                }

                return new DateTimeOffset((long)dbValue, TimeSpan.Zero);
            }
        };

        private static readonly TypeConverter _sqliteDataTimeOffsetConverter = new TypeConverter
        {
            DbType = DbType.String,
            Statement = "VARCHAR",

            TypeValueToDbValue = (typeValue, propertyType) =>
            {
                if (typeValue == null)
                {
                    return DBNull.Value;
                }

                //Microsoft.Data.Sqlite会自动ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture);
                return typeValue;
            },

            DbValueToTypeValue = (dbValue, propertyType) =>
            {
                Type dbValueType = dbValue.GetType();

                if (dbValueType == typeof(DBNull))
                {
                    return default(DateTimeOffset);
                }

                return DateTimeOffset.Parse(dbValue.ToString(), CultureInfo.InvariantCulture);
            }
        };

        static TypeConvert()
        {
            //解决MySql最多存储到Datetime(6)，而.net里为Datetime(7)
            RegisterGlobalTypeConverter(typeof(DateTimeOffset), _mysqlDataTimeOffsetConverter, DatabaseEngineType.MySQL);

            RegisterGlobalTypeConverter(typeof(DateTimeOffset), _sqliteDataTimeOffsetConverter, DatabaseEngineType.SQLite);
        }

        public static void RegisterGlobalTypeConverter(Type type, TypeConverter dbTypeInfo, DatabaseEngineType engineType)
        {
            switch (engineType)
            {
                case DatabaseEngineType.MySQL:
                    _mysqlGlobalConverters[type] = dbTypeInfo;
                    break;
                case DatabaseEngineType.SQLite:
                    _sqliteGlobalConverters[type] = dbTypeInfo;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Entity的Type值. 逻辑同EntityMapperCreator一致
        /// </summary>
        public static object? DbValueToTypeValue(object dbValue, DatabaseEntityPropertyDef propertyDef, DatabaseEngineType engineType) //Type targetType)
        {
            //查看属性TypeConverter
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbValueToTypeValue!(dbValue, propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;
            object? typeValue;

            //查看全局TypeConverter
            TypeConverter? globalConverter = GetGlobalConverter(trueType, engineType);

            if (globalConverter != null && globalConverter.DbValueToTypeValue != null)
            {
                typeValue = globalConverter.DbValueToTypeValue(dbValue, trueType);
            }
            else
            {
                //默认
                Type dbValueType = dbValue.GetType();

                if (dbValueType == typeof(DBNull))
                {
                    return default;
                }

                if (trueType.IsEnum)
                {
                    typeValue = Enum.Parse(trueType, dbValue.ToString(), true);
                }
                else if (trueType != dbValueType)
                {
                    typeValue = Convert.ChangeType(dbValue, trueType, CultureInfo.InvariantCulture);
                }
                else
                {
                    typeValue = dbValue;
                }
            }

            //处理Nullable
            if (propertyDef.NullableUnderlyingType == null)
            {
                return typeValue;
            }

            ConstructorInfo ctor = propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType });

            return ctor.Invoke(new object?[] { typeValue });
        }

        public static object TypeValueToDbValue(object? typeValue, DatabaseEntityPropertyDef propertyDef, DatabaseEngineType engineType)
        {
            //查看当前Property的TypeConvert
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.TypeValueToDbValue!(typeValue, propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert

            TypeConverter? globalConverter = GetGlobalConverter(trueType, engineType);

            if (globalConverter != null && globalConverter.TypeValueToDbValue != null)
            {
                return globalConverter.TypeValueToDbValue(typeValue, trueType);
            }

            //默认
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            if (trueType.IsEnum)
            {
                return typeValue.ToString();
            }

            return typeValue;
        }

        /// <summary>
        /// 没有考虑属性自定义的TypeConvert
        /// </summary>
        /// <param name="typeValue"></param>
        /// <param name="quotedIfNeed"></param>
        /// <param name="engineType"></param>
        /// <returns></returns>
        public static string TypeValueToDbValueStatement(object? typeValue, bool quotedIfNeed, DatabaseEngineType engineType)
        {
            if (typeValue == null)
            {
                return "null";
            }

            Type type = typeValue.GetType();
            DatabaseEntityPropertyDef propertyDef = new DatabaseEntityPropertyDef
            {
                Type = type,
                NullableUnderlyingType = Nullable.GetUnderlyingType(type),
                TypeConverter = null
            };

            object dbValue = TypeValueToDbValue(typeValue, propertyDef, engineType);

            string statement = dbValue switch
            {
                //null => "null",
                //Enum e => e.ToString(),
                DBNull _ => "null",
                DateTime _ => throw new DatabaseException(ErrorCode.UseDateTimeOffsetOnly),
                DateTimeOffset dt => dt.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture),
                bool b => b ? "1" : "0",
                _ => dbValue.ToString()
            };

            if (!quotedIfNeed || statement == "null" || !SqlHelper.IsValueNeedQuoted(dbValue!.GetType()))
            {
                return statement;
            }

            return SqlHelper.GetQuoted(statement);
        }

        public static DbType TypeToDbType(DatabaseEntityPropertyDef propertyDef, DatabaseEngineType engineType)
        {
            //查看属性的TypeConvert
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbType;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            TypeConverter? globalConverter = GetGlobalConverter(trueType, engineType);

            if (globalConverter != null)
            {
                return globalConverter.DbType;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return DbType.String;
            }

            throw new DatabaseException(ErrorCode.DatabaseUnSupported, $"Unspoorted Type:{propertyDef.NullableUnderlyingType ?? propertyDef.Type}, Property:{propertyDef.Name}, Entity:{propertyDef.EntityDef.EntityFullName}");
        }

        public static string TypeToDbTypeStatement(DatabaseEntityPropertyDef propertyDef, DatabaseEngineType engineType)
        {
            //查看属性自定义
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.Statement;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            TypeConverter? globalConverter = GetGlobalConverter(trueType, engineType);

            if (globalConverter != null)
            {
                return globalConverter.Statement;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return GetGlobalConverter(typeof(string), engineType)!.Statement;
            }

            throw new DatabaseException(ErrorCode.DatabaseUnSupported, $"Unspoorted Type:{propertyDef.NullableUnderlyingType ?? propertyDef.Type}, Property:{propertyDef.Name}, Entity:{propertyDef.EntityDef.EntityFullName}");
        }

        public static TypeConverter? GetGlobalConverter(Type trueType, DatabaseEngineType engineType)
        {
            Dictionary<Type, TypeConverter> typeConvertSettings = engineType switch
            {
                DatabaseEngineType.MySQL => _mysqlGlobalConverters,
                DatabaseEngineType.SQLite => _sqliteGlobalConverters,
                _ => throw new NotImplementedException(),
            };

            if (typeConvertSettings.TryGetValue(trueType, out TypeConverter globalTypeSetting))
            {
                return globalTypeSetting;
            }

            return null;
        }
    }
}