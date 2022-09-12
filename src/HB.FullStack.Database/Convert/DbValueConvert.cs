
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.SQL;

namespace HB.FullStack.Database.Convert
{
    internal static class DbValueConvert
    {
        private class DbTypeMapping
        {
            /// <summary>
            /// 不同数据库不同
            /// </summary>
            public string DbTypeStatement { get; set; } = null!;

            public DbType DbType { get; set; }

            /// <summary>
            /// 为null，表明不需要转换，直接拆箱装箱即可
            /// </summary>
            public IDbValueConverter? DbValueConverter { get; set; }
        }

        private static readonly Dictionary<Type, DbTypeMapping> _mysqlGlobalConverterInfos = new Dictionary<Type, DbTypeMapping>
        {
            [typeof(byte)] = new DbTypeMapping { DbTypeStatement = "TINYINT", DbType = DbType.Byte },
            [typeof(sbyte)] = new DbTypeMapping { DbTypeStatement = "TINYINT", DbType = DbType.SByte },
            [typeof(short)] = new DbTypeMapping { DbTypeStatement = "SMALLINT", DbType = DbType.Int16 },
            [typeof(ushort)] = new DbTypeMapping { DbTypeStatement = "SMALLINT", DbType = DbType.UInt16 },
            [typeof(int)] = new DbTypeMapping { DbTypeStatement = "INT", DbType = DbType.Int32 },
            [typeof(uint)] = new DbTypeMapping { DbTypeStatement = "INT", DbType = DbType.UInt32 },
            [typeof(long)] = new DbTypeMapping { DbTypeStatement = "BIGINT", DbType = DbType.Int64 },
            [typeof(ulong)] = new DbTypeMapping { DbTypeStatement = "BIGINT", DbType = DbType.UInt64 },
            [typeof(float)] = new DbTypeMapping { DbTypeStatement = "FLOAT", DbType = DbType.Single },
            [typeof(double)] = new DbTypeMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new DbTypeMapping { DbTypeStatement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new DbTypeMapping { DbTypeStatement = "TINYINT", DbType = DbType.Boolean },
            [typeof(string)] = new DbTypeMapping { DbTypeStatement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new DbTypeMapping { DbTypeStatement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new DbTypeMapping { DbTypeStatement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new DbTypeMapping { DbTypeStatement = "DATETIME(6)", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new DbTypeMapping { DbTypeStatement = "Binary", DbType = DbType.Binary }
        };

        private static readonly Dictionary<Type, DbTypeMapping> _sqliteGlobalConverterInfos = new Dictionary<Type, DbTypeMapping>
        {
            [typeof(byte)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.Byte },
            [typeof(sbyte)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.SByte },
            [typeof(short)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int16 },
            [typeof(ushort)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt16 },
            [typeof(int)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int32 },
            [typeof(uint)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt32 },
            [typeof(long)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int64 },
            [typeof(ulong)] = new DbTypeMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt64 },
            [typeof(float)] = new DbTypeMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Single },
            [typeof(double)] = new DbTypeMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new DbTypeMapping { DbTypeStatement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new DbTypeMapping { DbTypeStatement = "BOOL", DbType = DbType.Boolean },
            [typeof(string)] = new DbTypeMapping { DbTypeStatement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new DbTypeMapping { DbTypeStatement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new DbTypeMapping { DbTypeStatement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new DbTypeMapping { DbTypeStatement = "VARCHAR", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new DbTypeMapping { DbTypeStatement = "BLOB", DbType = DbType.Binary }
        };

        static DbValueConvert()
        {
            #region MySQL

            //解决MySql最多存储到Datetime(6)，而.net里为Datetime(7)
            RegisterGlobalDbValueConverter(typeof(DateTimeOffset), new MySqlDateTimeOffsetTypeConverter(), EngineType.MySQL);

            //解决MySql存储Guid的问题，存储为Binary(16)
            RegisterGlobalDbValueConverter(typeof(Guid), new MySqlGuidTypeConverter(), EngineType.MySQL);

            RegisterGlobalDbValueConverter(typeof(SimpleDate), new SimpleDateTypeConverter(), EngineType.MySQL);
            RegisterGlobalDbValueConverter(typeof(Time24Hour), new Time24HourTypeConverter(), EngineType.MySQL);

            #endregion

            #region SQLite

            RegisterGlobalDbValueConverter(typeof(DateTimeOffset), new SqliteDateTimeOffsetTypeConverter(), EngineType.SQLite);
            RegisterGlobalDbValueConverter(typeof(Guid), new SqliteGuidTypeConverter(), EngineType.SQLite);
            RegisterGlobalDbValueConverter(typeof(SimpleDate), new SimpleDateTypeConverter(), EngineType.SQLite);
            RegisterGlobalDbValueConverter(typeof(Time24Hour), new Time24HourTypeConverter(), EngineType.SQLite);

            #endregion
        }

        public static void RegisterGlobalDbValueConverter(Type type, IDbValueConverter typeConverter, EngineType engineType)
        {
            Dictionary<Type, DbTypeMapping> globalConverterInfos = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotSupportedException(),
            };

            DbTypeMapping converterInfo = new DbTypeMapping
            {
                DbType = typeConverter.DbType,
                DbTypeStatement = typeConverter.DbTypeStatement,
                DbValueConverter = typeConverter
            };

            globalConverterInfos[type] = converterInfo;
        }

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Model的Type值. 逻辑同ModelMapperCreator一致
        /// </summary>
        public static object? DbValueToTypeValue(object dbValue, DbModelPropertyDef propertyDef, EngineType engineType) //Type targetType)
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
            IDbValueConverter? globalConverter = GetGlobalDbValueConverter(trueType, engineType);

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
                    typeValue = System.Convert.ChangeType(dbValue, trueType, GlobalSettings.Culture);
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
        public static object TypeValueToDbValue(object? typeValue, DbModelPropertyDef? propertyDef, EngineType engineType)
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
            IDbValueConverter? globalConverter = GetGlobalDbValueConverter(trueType, engineType);

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
            DbModelPropertyDef propertyDef = new DbModelPropertyDef
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

        public static DbType TypeToDbType(DbModelPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性的TypeConvert
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbType;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            DbTypeMapping? converterInfo = GetGlobalDbTypeMapping(trueType, engineType);

            if (converterInfo != null)
            {
                return converterInfo.DbType;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return DbType.String;
            }

            throw DatabaseExceptions.ModelHasNotSupportedPropertyType(type: propertyDef.ModelDef.ModelFullName, propertyTypeName: (propertyDef.NullableUnderlyingType ?? propertyDef.Type).FullName, propertyName: propertyDef.Name);
        }

        public static string TypeToDbTypeStatement(DbModelPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性自定义
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbTypeStatement;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            DbTypeMapping? converterInfo = GetGlobalDbTypeMapping(trueType, engineType);

            if (converterInfo != null)
            {
                return converterInfo.DbTypeStatement;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return GetGlobalDbTypeMapping(typeof(string), engineType)!.DbTypeStatement;
            }
            throw DatabaseExceptions.ModelHasNotSupportedPropertyType(type: propertyDef.ModelDef.ModelFullName, propertyTypeName: (propertyDef.NullableUnderlyingType ?? propertyDef.Type).FullName, propertyName: propertyDef.Name);
        }

        public static IDbValueConverter? GetGlobalDbValueConverter(Type trueType, EngineType engineType)
        {
            return GetGlobalDbTypeMapping(trueType, engineType)?.DbValueConverter;
        }

        public static IDbValueConverter? GetGlobalDbValueConverter(Type trueType, int engineType)
        {
            return GetGlobalDbTypeMapping(trueType, (EngineType)engineType)?.DbValueConverter;
        }

        private static DbTypeMapping? GetGlobalDbTypeMapping(Type trueType, EngineType engineType)
        {
            Dictionary<Type, DbTypeMapping> typeConvertSettings = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotImplementedException(),
            };

            if (typeConvertSettings.TryGetValue(trueType, out DbTypeMapping? converterInfo))
            {
                return converterInfo;
            }

            return null;
        }
    }
}