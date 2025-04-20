using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static partial class PropertyNameValueExtensions
    {
        private static readonly ConcurrentDictionary<string, Func<object, PropertyNameValue[]>> _cache = new ConcurrentDictionary<string, Func<object, PropertyNameValue[]>>();

        public static PropertyNameValue[] GetPropertyNameValuesByAttribute<TAttr>(this object obj) where TAttr : Attribute
        {
            if (obj == null)
            {
                return Array.Empty<PropertyNameValue>();
            }

            Func<object, PropertyNameValue[]> func = GetCachedGetPropertyValuesFunc<TAttr>(obj.GetType());

            return func(obj);
        }

        private static Func<object, PropertyNameValue[]> GetCachedGetPropertyValuesFunc<TAttr>(Type objType) where TAttr : Attribute
        {
            string key = $"{objType.FullName}.{typeof(TAttr).Name}";

            return _cache.GetOrAdd(key, _ =>
            {
                IList<PropertyInfo> propertyInfos = objType.GetPropertyInfosByAttribute<TAttr>();

                return MetaAccess.CreateGetPropertyValuesDelegate2(objType, propertyInfos);
            });
        }
    }
}
