using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System
{
    //TODO:查看是否使用TypeConverter代替
    public static class StringConvertCenter
    {
        private static readonly Dictionary<Type, IStringConverter> _stringConverters = new Dictionary<Type, IStringConverter>
        {
            [typeof(string)] = new StringStringConverter(),
            [typeof(int)] = new IntStringConverter(),
            [typeof(long)] = new LongStringConverter(),
            [typeof(Guid)] = new GuidStringConverter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetConverter()
        };

        public static void RegisterStringConverter(Type type, IStringConverter stringConverter)
        {
            _stringConverters[type] = stringConverter;
        }

        public static bool HasStringConverter(this Type type)
        {
            return _stringConverters.ContainsKey(type);
        }

        [return: NotNullIfNotNull(nameof(value))]
        public static string? ToStringFrom<T>(this T? value, StringConvertPurpose purpose)
        {
            return ToStringFrom(value, typeof(T), purpose);
        }

        [return: NotNullIfNotNull(nameof(value))]
        public static string? ToStringFrom(this object? value, StringConvertPurpose purpose)
        {
            return value is null ? null : ToStringFrom(value, value.GetType(), purpose);
        }

        //warning: 不要更改签名
        [return: NotNullIfNotNull(nameof(value))]
        public static string? ToStringFrom(object? value, Type? valueType, StringConvertPurpose purpose)
        {
            if (value == null && valueType == null)
            {
                return null;
            }

            valueType ??= value!.GetType();

            if (valueType.IsEnum)
            {
                return value?.ToString();
            }

            if (!_stringConverters.TryGetValue(valueType, out IStringConverter? stringConverter))
            {
                //TODO: 考虑支持TypeConverter converter = TypeDescriptor.GetConverter(type);

                //TODO: 考虑直接返回ToString，如果没有converter的话

                throw new NotImplementedException($"不支持这种Type的ConvertToString. Type:{valueType.FullName}");

                //return value?.ToString();
            }

            return stringConverter!.ConvertToString(value, purpose);
        }

        public static T? FromStringTo<T>(this string? str, StringConvertPurpose purpose)
        {
            Type type = typeof(T);

            if (type.IsEnum)
            {
                return (T)Enum.Parse(type, str!);
            }

            if (!_stringConverters.TryGetValue(type, out IStringConverter? stringConverter))
            {
                //TODO: 考虑支持TypeConverter converter = TypeDescriptor.GetConverter(type);

                //TODO: 考虑直接返回ToString，如果没有converter的话

                throw new NotImplementedException($"不支持这种Type的ConvertToString. Type:{type.FullName}");

                //return value?.ToString();
            }

            return (T?)stringConverter.ConvertFromString(str, purpose);
        }

    }
}
