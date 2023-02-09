using System;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel;

namespace HB.FullStack.Database.Config
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
    }
}
