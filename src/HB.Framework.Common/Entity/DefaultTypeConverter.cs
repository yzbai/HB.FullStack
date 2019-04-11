using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HB.Framework.Common.Entity
{
    public static class DefaultTypeConverter
    {
        private static readonly Dictionary<Type, Func<object, object>> convertFunDict = new Dictionary<Type, Func<object, object>>();

        static DefaultTypeConverter()
        {
            #region type to type

            convertFunDict[typeof(byte)] = o => { return Convert.ToByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(sbyte)] = o => { return Convert.ToSByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(short)] = o => { return Convert.ToInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ushort)] = o => { return Convert.ToUInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(int)] = o => { return Convert.ToInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(uint)] = o => { return Convert.ToUInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(long)] = o => { return Convert.ToInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ulong)] = o => { return Convert.ToUInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(float)] = o => { return Convert.ToSingle(o, GlobalSettings.Culture); };
            convertFunDict[typeof(double)] = o => { return Convert.ToDouble(o, GlobalSettings.Culture); };
            convertFunDict[typeof(decimal)] = o => { return Convert.ToDecimal(o, GlobalSettings.Culture); };
            convertFunDict[typeof(bool)] = o => { return Convert.ToBoolean(o, GlobalSettings.Culture); };
            convertFunDict[typeof(string)] = o => { return Convert.ToString(o, GlobalSettings.Culture); };
            convertFunDict[typeof(char)] = o => { return Convert.ToChar(o, GlobalSettings.Culture); };
            convertFunDict[typeof(Guid)] = o => { return Guid.Parse(o.ToString()); };
            convertFunDict[typeof(DateTime)] = o => { return Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(DateTimeOffset)] = o => { return (DateTimeOffset)DateTime.SpecifyKind(Convert.ToDateTime(o, GlobalSettings.Culture), DateTimeKind.Utc); };
            convertFunDict[typeof(TimeSpan)] = o => { return Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(byte[])] = o => { return JsonUtil.Serialize(o); };
            convertFunDict[typeof(byte?)] = o => { return o == null ? null : (object)Convert.ToByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(sbyte?)] = o => { return o == null ? null : (object)Convert.ToSByte(o, GlobalSettings.Culture); };
            convertFunDict[typeof(short?)] = o => { return o == null ? null : (object)Convert.ToInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ushort?)] = o => { return o == null ? null : (object)Convert.ToUInt16(o, GlobalSettings.Culture); };
            convertFunDict[typeof(int?)] = o => { return o == null ? null : (object)Convert.ToInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(uint?)] = o => { return o == null ? null : (object)Convert.ToUInt32(o, GlobalSettings.Culture); };
            convertFunDict[typeof(long?)] = o => { return o == null ? null : (object)Convert.ToInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(ulong?)] = o => { return o == null ? null : (object)Convert.ToUInt64(o, GlobalSettings.Culture); };
            convertFunDict[typeof(float?)] = o => { return o == null ? null : (object)Convert.ToSingle(o, GlobalSettings.Culture); };
            convertFunDict[typeof(double?)] = o => { return o == null ? null : (object)Convert.ToDouble(o, GlobalSettings.Culture); };
            convertFunDict[typeof(decimal?)] = o => { return o == null ? null : (object)Convert.ToDecimal(o, GlobalSettings.Culture); };
            convertFunDict[typeof(bool?)] = o => { return o == null ? null : (object)Convert.ToBoolean(o, GlobalSettings.Culture); };
            convertFunDict[typeof(char?)] = o => { return o == null ? null : (object)Convert.ToChar(o, GlobalSettings.Culture); };
            convertFunDict[typeof(Guid?)] = o => { return o == null ? null : (object)Guid.Parse(o.ToString()); };
            convertFunDict[typeof(DateTime?)] = o => { return o == null ? null : (object)Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(DateTimeOffset?)] = o => { return o == null ? null : (DateTimeOffset?)DateTime.SpecifyKind(Convert.ToDateTime(o, GlobalSettings.Culture), DateTimeKind.Utc); };
            convertFunDict[typeof(TimeSpan?)] = o => { return o == null ? null : (object)Convert.ToDateTime(o, GlobalSettings.Culture); };
            convertFunDict[typeof(object)] = o => { return o ?? null; };
            convertFunDict[typeof(DBNull)] = o => { return o == null ? null : DBNull.Value; };

            #endregion
        }

        public static object DbValueToTypeValue(Type type, object dbValue)
        {
            if (type.IsEnum)
            {
                return Enum.Parse(type, dbValue.ToString(), true);
                //return Convert.ToInt32(value, GlobalSettings.Culture);
            }

            if (dbValue.GetType() == typeof(DBNull))
            {
                return type.IsValueType ? Activator.CreateInstance(type) : null;
            }

            if (type.IsAssignableFrom(typeof(IList<string>)))
            {
                return StringUtil.StringToList(dbValue.ToString());
            }

            if (type.IsAssignableFrom(typeof(IDictionary<string, string>)))
            {
                return StringUtil.StringToDictionary(dbValue.ToString());
            }

            Func<object, object> convertFn = convertFunDict[type];
            return convertFn(dbValue);
        }

        public static string TypeValueToDbValue(object value)
        {
            string valueStr;

            if (value != null)
            {
                Type type = value.GetType();

                if (type.IsEnum)
                {
                    valueStr = value.ToString();
                    //valueStr = ((Int32)value).ToString(GlobalSettings.Culture);
                }
                else if (typeof(IList<string>).IsAssignableFrom(type))
                {
                    valueStr = StringUtil.ListToString(value as IList<string>);
                }
                else if (typeof(IDictionary<string, string>).IsAssignableFrom(type))
                {
                    valueStr = StringUtil.DictionaryToString(value as IDictionary<string, string>);
                }
                else if (type == typeof(string))
                {
                    valueStr = (string)value;
                }
                else if (type == typeof(DateTime))
                {
                    valueStr = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss", GlobalSettings.Culture);
                }
                else if (type == typeof(DateTimeOffset))
                {
                    valueStr = ((DateTimeOffset)value).ToString("yyyy-MM-dd HH:mm:ss", GlobalSettings.Culture);
                }
                else if (type == typeof(bool))
                {
                    valueStr = (bool)value ? "1" : "0";
                }
                else if (type == typeof(DBNull))
                {
                    valueStr = null;
                }
                else
                {
                    valueStr = value.ToString();
                }
            }
            else
            {
                valueStr = null;
            }

            return valueStr;
        }
    }
}
