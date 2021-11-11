#nullable enable

using MessagePack;

using System.Buffers;
using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace System
{
    public static class SerializeUtil
    {
        #region Json

        //TODO: 使用source generator
        //https://devblogs.microsoft.com/dotnet/try-the-new-system-text-json-source-generator/

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
            //jsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }

        [return: NotNullIfNotNull("entity")]
        public static string? ToJson(object? entity)
        {
            return JsonSerializer.Serialize(entity, _jsonSerializerOptions);
        }

        public static bool TryToJson(object? entity, out string? json)
        {
            try
            {
                json = ToJson(entity);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                GlobalSettings.Logger?.LogSerializeLogError(entity?.GetType().FullName, ex);

                json = null;
                return false;
            }
        }

        [return: MaybeNull]
        public static T? FromJson<T>(string? jsonString)
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

        public static T? FromJsonForProperty<T>(string jsonString, string propertyName)
        {
            JsonNode? node = JsonNode.Parse(jsonString)?[propertyName];

            return node == null ? default : node.GetValue<T>();

            ////TODO: 使用JsonNode改写
            //JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            //JsonElement rootElement = jsonDocument.RootElement;

            //if (rootElement.TryGetProperty(name, out JsonElement jsonElement))
            //{
            //    return jsonElement.GetString();
            //}

            //return null;
        }

        private static readonly Type _collectionType = typeof(IEnumerable);

        /// <summary>
        /// 返回是否成功解析，
        /// 有可能成功解析，但结果是null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static bool TryFromJsonWithCollectionCheck<T>(string? jsonString, out T? target) where T : class
        {
            //if json begine with '[', and T is a array or can be assignable to IEnumerable<T> ,ok
            //if json begin with '[', and T is not array or can be assignable to IEnumerable<T>, but only one ok, not only one null
            //if json not begin with '[', T is array or ..., ok
            //if json not begin with '[', T is not array or ...., ok

            Type targetType = typeof(T);

            try
            {
                if (_collectionType.IsAssignableFrom(targetType))
                {
                    //target is collection
                    if (jsonString != null && jsonString.StartsWith('['))
                    {
                        target = FromJson<T>(jsonString);
                        return true;
                    }
                    else
                    {
                        Type argumentType = targetType.GetGenericArguments()[0];

                        IList lst = (IList)(typeof(List<>).MakeGenericType(argumentType).GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<Type>())!);

                        object? result = FromJson(argumentType, jsonString);

                        if (result != null)
                        {
                            lst.Add(result);
                        }

                        target = (T)lst;

                        return true;
                    }
                }
                else
                {
                    if (jsonString != null && jsonString.StartsWith('['))
                    {
                        Type genericType = typeof(IEnumerable<>).MakeGenericType(targetType);

                        IEnumerable<T>? lst = FromJson<IEnumerable<T>>(jsonString);

                        if (lst != null && lst.Count() == 1)
                        {
                            target = lst.ElementAt(0);
                            return true;
                        }
                        else if (lst != null && !lst.Any())
                        {
                            target = null;
                            return true;
                        }
                        else
                        {
                            target = null;
                            return false;
                        }
                    }
                    else
                    {
                        target = FromJson<T>(jsonString);
                        return true;
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                GlobalSettings.Logger?.LogTryDeserializeWithCollectionCheckError(jsonString, targetType.FullName, ex);
                target = null;
                return false;
            }
        }

        #endregion Json

        #region BinaryFormatter Serialize

        //https://blog.marcgravell.com/2020/03/why-do-i-rag-on-binaryformatter.html

        #endregion BinaryFormatter Serialize

        #region MsgPack Serialize

        public static byte[] Serialize<T>(T? t)
        {
            //MessagePack可以处理null
            return MessagePackSerializer.Serialize<T>(t!);
        }

        public static IEnumerable<byte[]> Serialize<T>(IEnumerable<T?> ts)
        {
            foreach (T? t in ts)
            {
                yield return MessagePackSerializer.Serialize<T>(t!);
            }
        }

        public static T? Deserialize<T>(byte[]? bytes)
        {
            if (bytes.IsNullOrEmpty())
            {
                return default;
            }

            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        public static IEnumerable<T?> Deserialize<T>(IEnumerable<byte[]?> bytess)
        {
            foreach (byte[]? bytes in bytess)
            {
                yield return Deserialize<T>(bytes);
            }
        }

        #endregion MsgPack Serialize
    }
}