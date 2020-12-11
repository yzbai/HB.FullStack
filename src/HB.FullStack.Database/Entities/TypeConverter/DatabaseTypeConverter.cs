#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

using HB.FullStack.Database;
using HB.FullStack.Database.Engine;
using HB.FullStack.Database.Entities;

namespace System
{
    internal static class DatabaseTypeConverter
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

        /// <summary>
        /// 将DataReader.GetValue(i)得到的数据库值，转换为Entity的Type值
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

        public static DbType TypeToDbType(DatabaseEntityPropertyDef propertyDef)
        {
            if (propertyDef.TypeConverter != null)
            {
                return propertyDef.TypeConverter.TypeToDbType(propertyDef.Type);
            }

            if (_typeToDbTypeDict.TryGetValue(propertyDef.NullableUnderlyingType ?? propertyDef.Type, out DbType dbType))
            {
                return dbType;
            }

            throw new DatabaseException(ErrorCode.DatabaseUnSupportedType, $"Unspoorted Type:{propertyDef.NullableUnderlyingType ?? propertyDef.Type}, Property:{propertyDef.Name}, Entity:{propertyDef.EntityDef.EntityFullName}");
        }


    }
}