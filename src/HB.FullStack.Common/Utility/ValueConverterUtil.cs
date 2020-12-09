#nullable enable

using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;

namespace System
{
    public static class ValueConverterUtil
    {
        /// <summary>
        /// 将数据库的值转换为内存C#值
        /// 用在从数据库查询后，数据库值转为类型值
        /// </summary>
        /// <returns>The value to type value.</returns>
        /// <param name="targetType">想要转成的C#类型</param>
        /// <param name="dbValue">Db value.</param>
        [Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1801:Review unused parameters", Justification = "<Pending>")]
        public static object? DbValueToTypeValue(object dbValue, Type targetType)
        {
            if (dbValue.GetType() == typeof(DBNull))
            {
                //return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;
                return default;
            }

            Type? underType = Nullable.GetUnderlyingType(targetType);

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
                rt = new DateTimeOffset((DateTime)dbValue, TimeSpan.Zero);
            }
            else
            {
                rt = dbValue;
            }

            if (underType == null)
            {
                return rt;
            }

            ConstructorInfo ctor = targetType.GetConstructor(new Type[] { underType });

            return ctor.Invoke(new object[] { rt });
        }


        /// <summary>
        /// 将C#值转换为字符串，便于拼接SQL字符串. 如果value不为null且不为DBNull，则返回不为null
        /// </summary>
        public static string? TypeValueToStringValue(object? value)
        {
            return value switch
            {
                null => null,
                DBNull _ => null,
                string str => str,
                DateTime => throw new FrameworkException(ErrorCode.UseDateTime),
                bool b => b ? "1" : "0",
                DateTimeOffset dt => dt.ToString(CultureInfo.InvariantCulture),//dt.UtcTicks.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }
    }
}