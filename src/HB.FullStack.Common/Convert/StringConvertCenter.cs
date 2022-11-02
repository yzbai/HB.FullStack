using System;
using System.Collections.Generic;

using HB.FullStack.Common.Convert.Converters;

namespace HB.FullStack.Common.Convert
{
    public static class StringConvertCenter
    {

        private static readonly Dictionary<Type, IStringConverter?> _stringConverters = new Dictionary<Type, IStringConverter?>
        {
            [typeof(string)] = new StringStringConverter(),
            [typeof(int)] = new IntStringConverter(),
            [typeof(Guid)] = new GuidStringConverter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetConverter()
        };

        public static string? ConvertToString(object? value, Type? valueType, StringConvertPurpose purpose)
        {
            if (value == null && valueType == null)
            {
                return null;
            }

            if (valueType == null)
            {
                valueType = value!.GetType();
            }

            if (valueType.IsEnum)
            {
                return value?.ToString();
            }

            if (!_stringConverters.TryGetValue(valueType, out IStringConverter? stringConverter))
            {
                //TODO: 考虑支持TypeConverter converter = TypeDescriptor.GetConverter(type);

                //TODO: 考虑直接返回ToString，如果没有converter的话

                //throw new NotImplementedException($"不支持这种Type的ConvertToString. Type:{valueType.FullName}");

                return value?.ToString();
            }

            return stringConverter!.ConvertToString(value, purpose);
        }

        public static void RegisterStringConverter(Type type, IStringConverter stringConverter)
        {
            _stringConverters[type] = stringConverter;
        }

        
    }
}
