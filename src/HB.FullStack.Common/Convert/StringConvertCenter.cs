using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using HB.FullStack.Common.Convert.Converters;

namespace HB.FullStack.Common.Convert
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

        [return: NotNullIfNotNull(nameof(value))]
        public static string? ToStrngFrom<T>(T? value, StringConvertPurpose purpose)
        {
            return ToStringFrom(typeof(T), value, purpose);
        }

        [return: NotNullIfNotNull(nameof(value))]
        public static string? ToStringFrom(Type? valueType, object? value, StringConvertPurpose purpose)
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

        public static T? FromStringTo<T>(string? str, StringConvertPurpose purpose)
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
