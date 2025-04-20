using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace System
{
    public static class PropertyDelegateExtensions
    {
        private static readonly ConcurrentDictionary<string, Func<object, object?>> _propertyGetDelegateCache = new ConcurrentDictionary<string, Func<object, object?>>();
        private static readonly ConcurrentDictionary<string, Action<object, object?>> _propertySetDelegateCache = new ConcurrentDictionary<string, Action<object, object?>>();
        private static readonly ConcurrentDictionary<string, Func<object, NameValuePair[]>> _cache = new ConcurrentDictionary<string, Func<object, NameValuePair[]>>();

        public static Func<object, object?> GetGetDelegate(this PropertyInfo? property)
        {
            if (property == null)
            {
                return _ => null;
            }

            string key = $"{property.DeclaringType!.FullName}_{property.Name}";

            return _propertyGetDelegateCache.GetOrAdd(key, _ => PropertyDelegateCreator.CreateGetDelegate(property));
        }

        public static Action<object, object?> GetSetDelegate(this PropertyInfo? property)
        {
            if (property == null)
            {
                return (_, _) => { };
            }

            string key = $"{property.DeclaringType!.FullName}_{property.Name}";

            return _propertySetDelegateCache.GetOrAdd(key, _ => PropertyDelegateCreator.CreateSetDelegate(property));
        }

        public static NameValuePair[] GetPropertyNameValuesByAttribute<TAttr>(this object? obj) where TAttr : Attribute
        {
            if (obj == null)
            {
                return [];
            }

            Type type = obj.GetType();

            string key = $"{type.FullName}.{typeof(TAttr).Name}";

            var func = _cache.GetOrAdd(key, _ =>
            {
                IList<PropertyInfo> propertyInfos = type.GetPropertyInfosByAttribute<TAttr>();

                return PropertyDelegateCreator.CreateBatchGetDelegate2(propertyInfos, type);
            });

            return func(obj);
        }
    }
}
