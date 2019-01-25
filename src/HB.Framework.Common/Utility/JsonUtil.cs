using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class JsonUtil
    {
        #region Json

        public static string ToJson(object domain)
        {
            return JsonConvert.SerializeObject(domain);
        }

        public static T FromJson<T>(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static object FromJson(Type type, string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }

            return JsonConvert.DeserializeObject(jsonString, type);
        }

        #endregion

        #region object to bytes

        public static T DeSerialize<T>(byte[] buffer)
        {
            if (buffer == null)
            {
                return default(T);
            }

            return FromJson<T>(Encoding.UTF8.GetString(buffer));
        }

        public static object DeSerialize(Type type, byte[] buffer)
        {
            if (buffer == null)
            {
                return null;
            }

            return FromJson(type, Encoding.UTF8.GetString(buffer));
        }


        public static byte[] Serialize(object item)
        {
            if (item == null)
            {
                return null;
            }

            return Encoding.UTF8.GetBytes(ToJson(item));
        }

        #endregion
    }
}
