using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel;
using System.Globalization;

namespace System
{
    /// <summary>
    /// To avoid pass long string all around
    /// </summary>
    [JsonConverter(typeof(ConnectionStringJsonConverter))]
    [TypeConverter(typeof(ConnectionStringTypeConverter))]
    public partial class ConnectionString
    {
        private readonly string _connectionString;

        [JsonConstructor]
        public ConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        public override string ToString()
        {
            return _connectionString;
        }

        public class ConnectionStringJsonConverter : JsonConverter<ConnectionString>
        {
            public override ConnectionString? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                string? text = reader.GetString();

                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }

                return new ConnectionString(text);
            }

            public override void Write(Utf8JsonWriter writer, ConnectionString value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString());
            }
        }

        /// <summary>
        /// 定义String和ConnectionString之间的相互转换, 用于ConfigurationBinder.TryConvertValue
        /// </summary>
        public class ConnectionStringTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            {
                if (sourceType == typeof(string)) { return true; }
                return base.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            {
                if (destinationType == typeof(string)) { return true; }
                return base.CanConvertTo(context, destinationType);
            }

            public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
            {
                if (value is string text)
                {
                    return new ConnectionString(text);
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
            {
                if (destinationType == typeof(string) && value != null)
                {
                    return value.ToString();
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
