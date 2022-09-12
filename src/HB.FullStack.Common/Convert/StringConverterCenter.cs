using System;
using System.Collections.Generic;

using HB.FullStack.Common.Convert.Converters;

namespace HB.FullStack.Common.Convert
{
    public static class ConvertCenter
    {

        private static readonly Dictionary<Type, IStringConverter?> _stringConverters = new Dictionary<Type, IStringConverter?>
        {
            [typeof(string)] = new StringStringConverter(),
            [typeof(int)] = new IntStringConverter(),
            [typeof(Guid)] = new GuidStringConverter(),
            [typeof(DateTimeOffset)] = new DateTimeOffsetConverter()
        };

        public static string? ConvertToString(object? value, Type valueType, StringConvertPurpose purpose)
        {
            if (!_stringConverters.TryGetValue(valueType, out IStringConverter? stringConverter))
            {
                throw new NotImplementedException($"不支持这种Type的ConvertToString. Type:{valueType.FullName}");
            }

            return stringConverter!.ConvertToString(value, purpose);
        }

        public static void RegisterStringConverter(Type type, IStringConverter stringConverter)
        {
            _stringConverters[type] = stringConverter;
        }
    }
}
