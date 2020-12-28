using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace HB.FullStack.Common.Resources
{
    public class ResourceDef
    {
        public string EndpointName { get; internal set; } = null!;
        public string ApiVersion { get; internal set; } = null!;
        public string Name { get; internal set; } = null!;
    }

    public static class ResourceDefFactory
    {
        private static readonly ConcurrentDictionary<Type, ResourceDef> _defDict = new ConcurrentDictionary<Type, ResourceDef>();

        private static readonly object _defDictLocker = new object();

        public static ResourceDef Get<T>() where T : Resource
        {
            Type type = typeof(T);

            if (_defDict.TryGetValue(type, out ResourceDef def))
            {
                return def;
            }

            lock (_defDictLocker)
            {
                if (_defDict.TryGetValue(type, out def))
                {
                    return def;
                }

                def = CreateResourceDef(type);

                _defDict[type] = def;
            }

            return _defDict[type];
        }

        private static ResourceDef CreateResourceDef(Type type)
        {
            var attr = type.GetCustomAttribute<ApiAttribute>();

            if (attr == null)
            {
                throw new FrameworkException($"{type.FullName}缺少ApiAttribute属性标签.");
            }

            return new ResourceDef
            {
                ApiVersion = attr.Version,
                EndpointName = attr.EndPointName,
                Name = type.Name
            };
        }
    }
}
