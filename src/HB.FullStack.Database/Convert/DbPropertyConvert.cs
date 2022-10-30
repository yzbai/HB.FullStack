
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
    internal static class DbPropertyConvert
    {
        private class DbPropertyMapping
        {
            /// <summary>
            /// 不同数据库不同
            /// </summary>
            public string DbTypeStatement { get; set; } = null!;

            public DbType DbType { get; set; }

            /// <summary>
            /// 为null，表明不需要转换，直接拆箱装箱即可
            /// </summary>
            public IDbPropertyConverter? DbPropertyConverter { get; set; }
        }

        private static readonly Dictionary<Type, DbPropertyMapping> _mysqlGlobalConverterInfos = new Dictionary<Type, DbPropertyMapping>
        {
            [typeof(byte)] = new DbPropertyMapping { DbTypeStatement = "TINYINT", DbType = DbType.Byte },
            [typeof(sbyte)] = new DbPropertyMapping { DbTypeStatement = "TINYINT", DbType = DbType.SByte },
            [typeof(short)] = new DbPropertyMapping { DbTypeStatement = "SMALLINT", DbType = DbType.Int16 },
            [typeof(ushort)] = new DbPropertyMapping { DbTypeStatement = "SMALLINT", DbType = DbType.UInt16 },
            [typeof(int)] = new DbPropertyMapping { DbTypeStatement = "INT", DbType = DbType.Int32 },
            [typeof(uint)] = new DbPropertyMapping { DbTypeStatement = "INT", DbType = DbType.UInt32 },
            [typeof(long)] = new DbPropertyMapping { DbTypeStatement = "BIGINT", DbType = DbType.Int64 },
            [typeof(ulong)] = new DbPropertyMapping { DbTypeStatement = "BIGINT", DbType = DbType.UInt64 },
            [typeof(float)] = new DbPropertyMapping { DbTypeStatement = "FLOAT", DbType = DbType.Single },
            [typeof(double)] = new DbPropertyMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new DbPropertyMapping { DbTypeStatement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new DbPropertyMapping { DbTypeStatement = "TINYINT", DbType = DbType.Boolean },
            [typeof(string)] = new DbPropertyMapping { DbTypeStatement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new DbPropertyMapping { DbTypeStatement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new DbPropertyMapping { DbTypeStatement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new DbPropertyMapping { DbTypeStatement = "DATETIME(6)", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new DbPropertyMapping { DbTypeStatement = "Binary", DbType = DbType.Binary }
        };

        private static readonly Dictionary<Type, DbPropertyMapping> _sqliteGlobalConverterInfos = new Dictionary<Type, DbPropertyMapping>
        {
            [typeof(byte)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.Byte },
            [typeof(sbyte)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.SByte },
            [typeof(short)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int16 },
            [typeof(ushort)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt16 },
            [typeof(int)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int32 },
            [typeof(uint)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt32 },
            [typeof(long)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.Int64 },
            [typeof(ulong)] = new DbPropertyMapping { DbTypeStatement = "INTEGER", DbType = DbType.UInt64 },
            [typeof(float)] = new DbPropertyMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Single },
            [typeof(double)] = new DbPropertyMapping { DbTypeStatement = "DOUBLE", DbType = DbType.Double },
            [typeof(decimal)] = new DbPropertyMapping { DbTypeStatement = "DECIMAL", DbType = DbType.Decimal },
            [typeof(bool)] = new DbPropertyMapping { DbTypeStatement = "BOOL", DbType = DbType.Boolean },
            [typeof(string)] = new DbPropertyMapping { DbTypeStatement = "VARCHAR", DbType = DbType.String },
            [typeof(char)] = new DbPropertyMapping { DbTypeStatement = "CHAR", DbType = DbType.StringFixedLength },
            [typeof(Guid)] = new DbPropertyMapping { DbTypeStatement = "CHAR(36)", DbType = DbType.Guid },
            [typeof(DateTimeOffset)] = new DbPropertyMapping { DbTypeStatement = "VARCHAR", DbType = DbType.DateTimeOffset },
            [typeof(byte[])] = new DbPropertyMapping { DbTypeStatement = "BLOB", DbType = DbType.Binary }
        };

        static DbPropertyConvert()
        {
            #region MySQL

            //解决MySql最多存储到Datetime(6)，而.net里为Datetime(7)
            RegisterGlobalDbPropertyConverter(typeof(DateTimeOffset), new MySqlDateTimeOffsetDbPropertyConverter(), EngineType.MySQL);

            //解决MySql存储Guid的问题，存储为Binary(16)
            RegisterGlobalDbPropertyConverter(typeof(Guid), new MySqlGuidDbPropertyConverter(), EngineType.MySQL);

            RegisterGlobalDbPropertyConverter(typeof(SimpleDate), new SimpleDateDbPropertyConverter(), EngineType.MySQL);
            RegisterGlobalDbPropertyConverter(typeof(Time24Hour), new Time24HourDbPropertyConverter(), EngineType.MySQL);

            #endregion

            #region SQLite

            RegisterGlobalDbPropertyConverter(typeof(DateTimeOffset), new SqliteDateTimeOffsetDbPropertyConverter(), EngineType.SQLite);
            RegisterGlobalDbPropertyConverter(typeof(Guid), new SqliteGuidDbPropertyConverter(), EngineType.SQLite);
            RegisterGlobalDbPropertyConverter(typeof(SimpleDate), new SimpleDateDbPropertyConverter(), EngineType.SQLite);
            RegisterGlobalDbPropertyConverter(typeof(Time24Hour), new Time24HourDbPropertyConverter(), EngineType.SQLite);

            #endregion
        }

        public static void RegisterGlobalDbPropertyConverter(Type type, IDbPropertyConverter typeConverter, EngineType engineType)
        {
            Dictionary<Type, DbPropertyMapping> globalConverterInfos = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotSupportedException(),
            };

            DbPropertyMapping converterInfo = new DbPropertyMapping
            {
                DbType = typeConverter.DbType,
                DbTypeStatement = typeConverter.DbTypeStatement,
                DbPropertyConverter = typeConverter
            };

            globalConverterInfos[type] = converterInfo;
        }

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Model的Type值. 逻辑同ModelMapperCreator一致
        /// </summary>
        public static object? DbFieldValueToPropertyValue(object dbValue, DbModelPropertyDef propertyDef, EngineType engineType) //Type targetType)
        {
            Type dbValueType = dbValue.GetType();

            if (dbValueType == typeof(DBNull))
            {
                return null;
            }

            //查看属性TypeConverter
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbFieldValueToPropertyValue(dbValue, propertyDef.Type);
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;
            object? typeValue;

            //查看全局TypeConverter
            IDbPropertyConverter? globalConverter = GetGlobalDbPropertyConverter(trueType, engineType);

            if (globalConverter != null)
            {
                typeValue = globalConverter.DbFieldValueToPropertyValue(dbValue, trueType);
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
                    typeValue = System.Convert.ChangeType(dbValue, trueType, Globals.Culture);
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
        public static object PropertyValueToDbFieldValue(object? typeValue, DbModelPropertyDef? propertyDef, EngineType engineType)
        {
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            //查看当前Property的TypeConvert
            if (propertyDef?.TypeConverter != null)
            {
                return propertyDef.TypeConverter.PropertyValueToDbFieldValue(typeValue, propertyDef.Type);
            }

            Type trueType = propertyDef == null ? typeValue.GetType() : propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            IDbPropertyConverter? globalConverter = GetGlobalDbPropertyConverter(trueType, engineType);

            if (globalConverter != null)
            {
                return globalConverter.PropertyValueToDbFieldValue(typeValue, trueType);
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
        public static string DoNotUseUnSafePropertyValueToDbFieldValueStatement(object? typeValue, bool quotedIfNeed, EngineType engineType)
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

            object dbValue = PropertyValueToDbFieldValue(typeValue, propertyDef, engineType);

            string statement = dbValue switch
            {
                //null => "null",
                //Enum e => e.ToString(),
                DBNull _ => "null",
                DateTime _ => throw DatabaseExceptions.UseDateTimeOffsetOnly(),
                DateTimeOffset dt => dt.ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", Globals.Culture),
                bool b => b ? "1" : "0",
                _ => dbValue.ToString()!
            };

            if (!quotedIfNeed || statement == "null" || !SqlHelper.IsValueNeedQuoted(dbValue!.GetType()))
            {
                return statement;
            }

            return SqlHelper.GetQuoted(statement);
        }

        public static DbType PropertyTypeToDbType(DbModelPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性的TypeConvert
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbType;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            DbPropertyMapping? converterInfo = GetGlobalDbPropertyMapping(trueType, engineType);

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

        public static string PropertyTypeToDbTypeStatement(DbModelPropertyDef propertyDef, EngineType engineType)
        {
            //查看属性自定义
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.DbTypeStatement;
            }

            Type trueType = propertyDef.NullableUnderlyingType ?? propertyDef.Type;

            //查看全局TypeConvert
            DbPropertyMapping? converterInfo = GetGlobalDbPropertyMapping(trueType, engineType);

            if (converterInfo != null)
            {
                return converterInfo.DbTypeStatement;
            }

            //默认处理
            if (trueType.IsEnum)
            {
                return GetGlobalDbPropertyMapping(typeof(string), engineType)!.DbTypeStatement;
            }
            throw DatabaseExceptions.ModelHasNotSupportedPropertyType(type: propertyDef.ModelDef.ModelFullName, propertyTypeName: (propertyDef.NullableUnderlyingType ?? propertyDef.Type).FullName, propertyName: propertyDef.Name);
        }

        public static IDbPropertyConverter? GetGlobalDbPropertyConverter(Type trueType, EngineType engineType)
        {
            return GetGlobalDbPropertyMapping(trueType, engineType)?.DbPropertyConverter;
        }

        public static IDbPropertyConverter? GetGlobalDbPropertyConverter(Type trueType, int engineType)
        {
            return GetGlobalDbPropertyMapping(trueType, (EngineType)engineType)?.DbPropertyConverter;
        }

        private static DbPropertyMapping? GetGlobalDbPropertyMapping(Type trueType, EngineType engineType)
        {
            Dictionary<Type, DbPropertyMapping> typeConvertSettings = engineType switch
            {
                EngineType.MySQL => _mysqlGlobalConverterInfos,
                EngineType.SQLite => _sqliteGlobalConverterInfos,
                _ => throw new NotImplementedException(),
            };

            if (typeConvertSettings.TryGetValue(trueType, out DbPropertyMapping? converterInfo))
            {
                return converterInfo;
            }

            return null;
        }
    }
}