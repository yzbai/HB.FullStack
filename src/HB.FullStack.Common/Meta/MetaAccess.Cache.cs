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

        private static readonly Dictionary<string, Func<object, PropertyNameValue[]>> _getPropertyValuesFuncDict = new Dictionary<string, Func<object, PropertyNameValue[]>>();

        //TODO: do we need a lock?
        private static Func<object, PropertyNameValue[]> GetCachedGetPropertyValuesFunc<TAttr>(Type objType) where TAttr : Attribute
        {
            string key = $"{objType.FullName}.{typeof(TAttr).Name}";

            if (_getPropertyValuesFuncDict.TryGetValue(key, out Func<object, PropertyNameValue[]>? cachedFunc))
            {
                return cachedFunc;
            }

            IList<PropertyInfo> propertyInfos = ReflectionUtil.GetPropertyInfosByAttribute<TAttr>(objType);

            Func<object, PropertyNameValue[]> func = CreateGetPropertyValuesDelegate2(objType, propertyInfos);

            _getPropertyValuesFuncDict.TryAdd(key, func);

            return func;
        }

        public static PropertyNameValue[] GetPropertyValuesByAttribute<TAttr>(object obj) where TAttr : Attribute
        {
            if (obj == null)
            {
                return Array.Empty<PropertyNameValue>();
            }

            Func<object, PropertyNameValue[]> func = GetCachedGetPropertyValuesFunc<TAttr>(obj.GetType());

            return func(obj);
        }
    }
}
