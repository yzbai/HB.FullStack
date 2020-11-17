#nullable enable

using System.Collections.Generic;
using System.Globalization;

namespace System
{
    public static class ValueConverterUtil
    {
        private static readonly Dictionary<Type, Func<object, object?>> _convertFunDict = new Dictionary<Type, Func<object, object?>>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <exception cref="System.InvalidCastException">Ignore.</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">Ignore.</exception>
        static ValueConverterUtil()
        {
            #region type to type

            _convertFunDict[typeof(byte)] = o => { return Convert.ToByte(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(sbyte)] = o => { return Convert.ToSByte(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(short)] = o => { return Convert.ToInt16(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(ushort)] = o => { return Convert.ToUInt16(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(int)] = o => { return Convert.ToInt32(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(uint)] = o => { return Convert.ToUInt32(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(long)] = o => { return Convert.ToInt64(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(ulong)] = o => { return Convert.ToUInt64(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(float)] = o => { return Convert.ToSingle(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(double)] = o => { return Convert.ToDouble(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(decimal)] = o => { return Convert.ToDecimal(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(bool)] = o => { return Convert.ToBoolean(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(string)] = o => { return Convert.ToString(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(char)] = o => { return Convert.ToChar(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(Guid)] = o => { return Guid.Parse(o.ToString()); };
            //_convertFunDict[typeof(DateTime)] = o => { return DateTime.Parse(o.ToString(), CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(DateTimeOffset)] = o =>
            {
                long ticks = Convert.ToInt64(o, CultureInfo.InvariantCulture);
                return new DateTimeOffset(ticks, TimeSpan.Zero);
            };
            _convertFunDict[typeof(TimeSpan)] = o =>
            {
                long ticks = Convert.ToInt64(o, CultureInfo.InvariantCulture);

                return TimeSpan.FromTicks(ticks);
            };

            _convertFunDict[typeof(byte?)] = o => { return Convert.ToByte(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(sbyte?)] = o => { return Convert.ToSByte(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(short?)] = o => { return Convert.ToInt16(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(ushort?)] = o => { return Convert.ToUInt16(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(int?)] = o => { return Convert.ToInt32(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(uint?)] = o => { return Convert.ToUInt32(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(long?)] = o => { return Convert.ToInt64(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(ulong?)] = o => { return Convert.ToUInt64(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(float?)] = o => { return Convert.ToSingle(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(double?)] = o => { return Convert.ToDouble(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(decimal?)] = o => { return Convert.ToDecimal(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(bool?)] = o => { return Convert.ToBoolean(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(char?)] = o => { return Convert.ToChar(o, CultureInfo.InvariantCulture); };
            _convertFunDict[typeof(Guid?)] = o => { return Guid.Parse(o.ToString()); };
            //_convertFunDict[typeof(DateTime?)] = o => { return o == null ? null : new DateTime?(DateTime.Parse(o.ToString(), CultureInfo.InvariantCulture)); };
            _convertFunDict[typeof(DateTimeOffset?)] = o =>
            {
                if (o == null)
                {
                    return null;
                }

                long ticks = Convert.ToInt64(o, CultureInfo.InvariantCulture);
                return new DateTimeOffset(ticks, TimeSpan.Zero);
            };
            _convertFunDict[typeof(TimeSpan?)] = o =>
            {
                if (o == null)
                {
                    return null;
                }

                long ticks = Convert.ToInt64(o, CultureInfo.InvariantCulture);

                return TimeSpan.FromTicks(ticks);
            };

            _convertFunDict[typeof(byte[])] = o => { return SerializeUtil.PackAsync(o); };
            _convertFunDict[typeof(object)] = o => { return o ?? null; };
            _convertFunDict[typeof(DBNull)] = o => { return DBNull.Value; };

            #endregion type to type
        }

        /// <summary>
        /// 将数据库的值转换为内存C#值
        /// 用在从数据库查询后，数据库值转为类型值
        /// </summary>
        /// <returns>The value to type value.</returns>
        /// <param name="targetType">想要转成的C#类型</param>
        /// <param name="dbValue">Db value.</param>
        public static object? DbValueToTypeValue(object dbValue, Type targetType)
        {
            if (dbValue.GetType() == typeof(DBNull))
            {
                //return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                return default;
            }

            if (targetType.IsEnum)
            {
                return Enum.Parse(targetType, dbValue.ToString(), true);
            }

            if (targetType.IsAssignableFrom(typeof(IList<string>)))
            {
                return StringUtil.StringToList(dbValue.ToString());
            }

            if (targetType.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return StringUtil.StringToDictionary(dbValue.ToString());
            }

            Func<object, object?> convertFn = _convertFunDict[targetType];
            return convertFn(dbValue);
        }

        /// <summary>
        /// 将C#值转换为字符串，便于拼接SQL字符串. 如果value不为null且不为DBNull，则返回不为null
        /// </summary>
        public static string? TypeValueToStringValue(object? value)
        {
            if (value == null)
            {
                return null;
            }

            Type type = value.GetType();

            if (type.IsEnum)
            {
                return value.ToString();
            }

            return value switch
            {
                string str => str,
                Enum e => e.ToString(),
                DBNull _ => null,
                DateTime => throw new FrameworkException(ErrorCode.UseDateTime),
                //DateTimeOffset dt => dt.ToString(GlobalSettings.DateTimeFormat, CultureInfo.InvariantCulture),
                DateTimeOffset dt => dt.UtcTicks.ToString(CultureInfo.InvariantCulture),
                bool b => b ? "1" : "0",
                IList<string> lst => StringUtil.ListToString(lst),
                IDictionary<string, string> dict => StringUtil.DictionaryToString(dict),
                _ => value.ToString()
            };
        }
    }
}