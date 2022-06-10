

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

using HB.FullStack.Database.Entities;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;
using HB.FullStack.Common;

namespace HB.FullStack.Database.Converter
{
    internal static class TypeConvert
    {
        private class ConverterInfo
        {
            public string Statement { get; set; } = null!;

            public DbType DbType { get; set; }

            public ITypeConverter? TypeConverter { get; set; }
        }

        private static readonly Dictionary<Type, ConverterInfo> _mysqlGlobalConverterInfos = new Dictionary<Type, ConverterInfo>
        {
            [typeof(byte)] = new ConverterInfo { Statement = "TINYINT", DbType = DbType.Byte },
            [typeof(sbyte)] = new ConverterInfo { Statement = "TINYINT", DbType = DbType.SByte },
            [typeof(short)] = new ConverterInfo { Statement = "SMALLINT", DbType = DbType.Int16 },
            [typeof(ushort)] = new ConverterInfo { Statement = "SMALLINT", DbType = DbType.UInt16 },
            [typeof(int)] = new ConverterInfo { Statement = "INT", DbType = DbType.Int32 },
            [typeof(uint)] = new ConverterInfo { Statement = "INT", DbType = DbType.UInt32 },
            [typeof(long)] = new ConverterInfo { Statement = "BIGINT", DbType = DbType.Int64 },
            [typeof(ulong)] = new ConverterInfo { Statement = "BIGINT", DbType = DbType.UInt64 },
            [typeof(float)] = new ConverterInfo { Statement = "FLOAT", DbType = DbType.Single },
            [typeof(double)] = new ConverterInfo { Statement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new ConverterInfo { Statement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new ConverterInfo { Statement = "TINYINT", DbType = DbType.Boolean },
            [typeof(string)] = new ConverterInfo { Statement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new ConverterInfo { Statement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new ConverterInfo { Statement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new ConverterInfo { Statement = "DATETIME(6)", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new ConverterInfo { Statement = "Binary", DbType = DbType.Binary }
        };

        private static readonly Dictionary<Type, ConverterInfo> _sqliteGlobalConverterInfos = new Dictionary<Type, ConverterInfo>
        {
            [typeof(byte)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.Byte },
            [typeof(sbyte)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.SByte },
            [typeof(short)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.Int16 },
            [typeof(ushort)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.UInt16 },
            [typeof(int)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.Int32 },
            [typeof(uint)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.UInt32 },
            [typeof(long)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.Int64 },
            [typeof(ulong)] = new ConverterInfo { Statement = "INTEGER", DbType = DbType.UInt64 },
            [typeof(float)] = new ConverterInfo { Statement = "DOUBLE", DbType = DbType.Single },
            [typeof(double)] = new ConverterInfo { Statement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new ConverterInfo { Statement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new ConverterInfo { Statement = "BOOL", DbType = DbType.Boolean },
            [typeof(string)] = new ConverterInfo { Statement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new ConverterInfo { Statement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new ConverterInfo { Statement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new ConverterInfo { Statement = "VARCHAR", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new ConverterInfo { Statement = "BLOB", DbType = DbType.Binary }
        };

        static TypeConvert()
        {
            #region MySQL

            //解决MySql最多存储到Datetime(6)，而.net里为Datetime(7)
            RegisterGlobalTypeConverter(typeof(DateTimeOffset), new MySqlDateTimeOffsetTypeConverter(), EngineType.MySQL);

            //解决MySql存储Guid的问题，存储为Binary(16)
            RegisterGlobalTypeConverter(typeof(Guid), new MySqlGuidTypeConverter(), EngineType.MySQL);

            RegisterGlobalTypeConverter(typeof(SimpleDate), new SimpleDateTypeConverter(), EngineType.MySQL);
            RegisterGlobalTypeConverter(typeof(Time24Hour), new Time24HourTypeConverter(), EngineType.MySQL);

            #endregion

            #region SQLite

            RegisterGlobalTypeConverter(typeof(DateTimeOffset), new SqliteDateTimeOffsetTypeConverter(), EngineType.SQLite);
            RegisterGlobalTypeConverter(typeof(Guid), new SqliteGuidTypeConverter(), EngineType.SQLite);
            RegisterGlobalTypeConverter(typeof(SimpleDate), new SimpleDateTypeConverter(), EngineType.SQLite);
            RegisterGlobalTypeConverter(typeof(Time24Hour), new Time24HourTypeConverter(), EngineType.SQLite);

            #endregion
        }

        public static void RegisterGlobalTypeConverter(Type type, ITypeConverter typeConverter, EngineType engineType)
        {
            Dictionary<Type, ConverterInfo> globalConverterInfos = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotSupportedException(),
            };

            ConverterInfo converterInfo = new ConverterInfo
            {
                DbType = typeConverter.DbType,
                Statement = typeConverter.Statement,
                TypeConverter = typeConverter
            };

            globalConverterInfos[type] = converterInfo;
        }

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Entity的Type值. 逻辑同EntityMapperCreator一致
        /// </summary>
        public static object? DbValueToTypeValue(object dbValue, EntityPropertyDef propertyDef, EngineType engineType) //Type targetType)
        {
            Type dbValueType = dbValue.GetType();

            if (dbValueType == typeof(DBNull))
            {
                return null;
            }

            //查看属性TypeConverter
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbValueToTypeValue(dbValue, propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;
            object? typeValue;

            //查看全局TypeConverter
            ITypeConverter? globalConverter = GetGlobalTypeConverter(trueType, engineType);

            if (globalConverter != null)
            {
                typeValue = globalConverter.DbValueToTypeValue(dbValue, trueType);
            }
            else
            {
                //默认

                if (trueType.IsEnum)
                {
                    typeValue = Enum.Parse(trueType, dbValue.ToString()!, true);
                }
                else if (trueType != dbValueType)
                {
                    typeValue = Convert.ChangeType(dbValue, trueType, GlobalSettings.Culture);
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

            ConstructorInfo? ctor = propertyDef.Type.GetConstructor(new Type[] { propertyDef.NullableUnderlyingType });

            return ctor!.Invoke(new object?[] { typeValue });
        }

        /// <summary>
        /// propertyDef为null，则不考虑这个属性自定义的TypeConverter
        /// </summary>
        public static object TypeValueToDbValue(object? typeValue, EntityPropertyDef? propertyDef, EngineType engineType)
        {
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            //查看当前Property的TypeConvert
            if (propertyDef?.TypeConverter != null)
            {
                return propertyDef.TypeConverter.TypeValueToDbValue(typeValue, propertyDef.Type);
            }

            Type trueType = propertyDef == null ? typeValue.GetType() : propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert

            ITypeConverter? globalConverter = GetGlobalTypeConverter(trueType, engineType);

            if (globalConverter != null)
            {
                return globalConverter.TypeValueToDbValue(typeValue, trueType);
            }

            //默认
            if (trueType.IsEnum)
            {
                return typeValue.ToString()!;
            }

            return typeValue;
        }

        /// <summary>
        /// 没有考虑属性自定义的TypeConvert
        /// 有安全隐患,
        /// </summary>

        public static string DoNotUseUnSafeTypeValueToDbValueStatement(object? typeValue, bool quotedIfNeed, EngineType engineType)
        {
            if (typeValue == null)
            {
                return "null";
            }

            Type type = typeValue.GetType();
            EntityPropertyDef propertyDef = new EntityPropertyDef
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
                DateTime _ => throw DatabaseExceptions.UseDateTimeOffsetOnly(),
                DateTimeOffset dt => dt.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", GlobalSettings.Culture),
                bool b => b ? "1" : "0",
                _ => dbValue.ToString()!
            };

            if (!quotedIfNeed || statement == "null" || !SqlHelper.IsValueNeedQuoted(dbValue!.GetType()))
            {
                return statement;
            }

            return SqlHelper.GetQuoted(statement);
        }

        public static DbType TypeToDbType(EntityPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性的TypeConvert
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbType;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            ConverterInfo? converterInfo = GetGlobalConverterInfo(trueType, engineType);

            if (converterInfo != null)
            {
                return converterInfo.DbType;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return DbType.String;
            }

            throw DatabaseExceptions.EntityHasNotSupportedPropertyType(type: propertyDef.EntityDef.EntityFullName, propertyTypeName: (propertyDef.NullableUnderlyingType ?? propertyDef.Type).FullName, propertyName: propertyDef.Name);
        }

        public static string TypeToDbTypeStatement(EntityPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性自定义
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.Statement;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            ConverterInfo? converterInfo = GetGlobalConverterInfo(trueType, engineType);

            if (converterInfo != null)
            {
                return converterInfo.Statement;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return GetGlobalConverterInfo(typeof(string), engineType)!.Statement;
            }
            throw DatabaseExceptions.EntityHasNotSupportedPropertyType(type: propertyDef.EntityDef.EntityFullName, propertyTypeName: (propertyDef.NullableUnderlyingType ?? propertyDef.Type).FullName, propertyName: propertyDef.Name);
        }

        public static ITypeConverter? GetGlobalTypeConverter(Type trueType, EngineType engineType)
        {
            return GetGlobalConverterInfo(trueType, engineType)?.TypeConverter;
        }

        public static ITypeConverter? GetGlobalTypeConverter(Type trueType, int engineType)
        {
            return GetGlobalConverterInfo(trueType, (EngineType)engineType)?.TypeConverter;
        }

        private static ConverterInfo? GetGlobalConverterInfo(Type trueType, EngineType engineType)
        {
            Dictionary<Type, ConverterInfo> typeConvertSettings = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotImplementedException(),
            };

            if (typeConvertSettings.TryGetValue(trueType, out ConverterInfo? converterInfo))
            {
                return converterInfo;
            }

            return null;
        }
    }
}