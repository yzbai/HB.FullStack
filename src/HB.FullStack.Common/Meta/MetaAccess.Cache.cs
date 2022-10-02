using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.Common.Meta
{
    public static partial class MetaAccess
    {

        private static readonly Dictionary<string, Func<object, PropertyValue[]>> _getPropertyValuesFuncDict = new Dictionary<string, Func<object, PropertyValue[]>>();

        //TODO: do we need a lock?
        private static Func<object, PropertyValue[]> GetCachedGetPropertyValuesFunc<TAttr>(Type objType) where TAttr : Attribute
        {
            string key = $"{objType.FullName}.{typeof(TAttr).Name}";

            if (_getPropertyValuesFuncDict.TryGetValue(key, out Func<object, PropertyValue[]>? cachedFunc))
            {
                return cachedFunc;
            }

            IList<PropertyInfo> propertyInfos = ReflectionUtil.GetPropertyInfosByAttribute<TAttr>(objType);

            Func<object, PropertyValue[]> func = CreateGetPropertyValuesDelegate2(objType, propertyInfos);

            _getPropertyValuesFuncDict.TryAdd(key, func);

            return func;
        }

        public static PropertyValue[] GetPropertyValuesByAttribute<TAttr>(object obj) where TAttr : Attribute
        {
            if (obj == null)
            {
                return Array.Empty<PropertyValue>();
            }

            Func<object, PropertyValue[]> func = GetCachedGetPropertyValuesFunc<TAttr>(obj.GetType());

            return func(obj);
        }
    }
}
