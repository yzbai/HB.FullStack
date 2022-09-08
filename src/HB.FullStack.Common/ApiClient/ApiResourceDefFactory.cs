using System;
using System.Collections.Generic;
using System.Reflection;

using HB.FullStack.Common.Api;

namespace HB.FullStack.Common.ApiClient
{
    public static class ApiResourceDefFactory
    {
        private static readonly IDictionary<string, ApiResourceDef> _defs = new Dictionary<string, ApiResourceDef>();

        static ApiResourceDefFactory()
        {
            //TODO: Do we have a Pefermance problem ?

            IEnumerable<Type> allResTypes = ReflectionUtil.GetAllTypeByCondition(t => t.IsSubclassOf(typeof(ApiResource)) && !t.IsAbstract);

            foreach (Type type in allResTypes)
            {
                ApiResourceDef def = CreateApiResourceDef(type);

                _defs.Add(def.ResName, def);
            }
        }

        private static ApiResourceDef CreateApiResourceDef(Type type)
        {
            ApiResourceDef def = new ApiResourceDef();

            def.ResName = type.Name;

            //PropertyDefs
            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                ApiResourcePropertyDef propertyDef = CreateApiResourcePropertyDef(propertyInfo);

                def.PropertyDefs.Add(propertyDef.PropertyName, propertyDef);
            }

            return def;
        }

        private static ApiResourcePropertyDef CreateApiResourcePropertyDef(PropertyInfo propertyInfo)
        {
            ApiResourcePropertyDef propertyDef = new ApiResourcePropertyDef();

            propertyDef.PropertyName = propertyInfo.Name;

            var queryItemAttribute = propertyInfo.GetCustomAttribute<RequestDataAttribute>(true);

            if (queryItemAttribute != null)
            {
                propertyDef.IsQueryItem = true;
                propertyInfo.GetValue
            }

            return propertyDef;
        }

        public static ApiResourceDef? GetDef(string resName)
        {
            if (_defs.TryGetValue(resName, out ApiResourceDef? def))
            {
                return def;
            }

            return null;
        }
    }
}
