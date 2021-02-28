#nullable enable

using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using MsgPack.Serialization;

namespace System
{
    public static class SerializeUtil
    {
        #region Json

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions();

        static SerializeUtil()
        {
            Configure(_jsonSerializerOptions);
        }

        public static void Configure(JsonSerializerOptions jsonSerializerOptions)
        {
            jsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
        }

        [return: NotNullIfNotNull("entity")]
        public static string? ToJson(object? entity)
        {
            return JsonSerializer.Serialize(entity, _jsonSerializerOptions);
        }

        [return: MaybeNull]
        public static T FromJson<T>(string? jsonString)
        {
            return jsonString.IsNullOrEmpty() ? default : JsonSerializer.Deserialize<T>(jsonString, _jsonSerializerOptions);
        }

        public static async Task<object?> FromJsonAsync(Type dataType, Stream responseStream)
        {
            return await JsonSerializer.DeserializeAsync(responseStream, dataType, _jsonSerializerOptions).ConfigureAwait(false);
        }

        public static async Task<T?> FromJsonAsync<T>(Stream responseStream)
        {
            return await JsonSerializer.DeserializeAsync<T>(responseStream, _jsonSerializerOptions).ConfigureAwait(false);
        }


        public static object? FromJson(Type type, string? jsonString)
        {
            if (jsonString.IsNullOrEmpty())
            {
                return null;
            }

            return JsonSerializer.Deserialize(jsonString, type, _jsonSerializerOptions);
        }

        public static string? FromJson(string jsonString, string name)
        {
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            JsonElement rootElement = jsonDocument.RootElement;

            if (rootElement.TryGetProperty(name, out JsonElement jsonElement))
            {
                return jsonElement.GetString();
            }

            return null;
        }

        #endregion Json

        #region BinaryFormatter Serialize

        //https://blog.marcgravell.com/2020/03/why-do-i-rag-on-binaryformatter.html

        #endregion BinaryFormatter Serialize

        #region MsgPack Serialize

        public static async Task<byte[]> PackAsync<T>(T? t) where T : class
        {
            MessagePackSerializer<T> serializer = MessagePackSerializer.Get<T>();

#pragma warning disable CS8604 // Possible null reference argument.
            return await serializer.PackSingleObjectAsync(t).ConfigureAwait(false);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public static async Task<T?> UnPackAsync<T>(byte[]? bytes) where T : class
        {
            if (bytes.IsNullOrEmpty())
            {
                return null;
            }

            MessagePackSerializer<T> serializer = MessagePackSerializer.Get<T>();

            return await serializer.UnpackSingleObjectAsync(bytes).ConfigureAwait(false);
        }

        #endregion MsgPack Serialize
    }
}