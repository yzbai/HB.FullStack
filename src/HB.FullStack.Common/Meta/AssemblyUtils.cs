using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System
{
    public static class AssemblyUtils
    {
        public static IEnumerable<Assembly> GetAllAssemblies()
        {
            //AppDomain不会主动加载所有的Assembly，直到用到才加载。所以一开始可能不全
            return AppDomain.CurrentDomain.GetAssemblies();

            //这里存在问题，如果是File的方式，会导致Type的HashCode不一致
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //return Directory
            //    .GetFiles(path, "*.dll")
            //    .Select(f => Assembly.LoadFile(f));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(Func<Type, bool> condition)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => condition(t));

            //通过File的方式会导致Type的HashCode不一致
            //string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //return Directory
            //    .GetFiles(path, "*.dll")
            //    .SelectMany(file => Assembly.LoadFile(file).GetTypes())
            //    .Where(t => condition(t));
        }

        public static IEnumerable<Type> GetAllTypeByCondition(IList<string> assembliesToCheck, Func<Type, bool> condition)
        {
            return assembliesToCheck
                .SelectMany(assemblyName => Assembly.Load(assemblyName).GetTypes())
                .Where(t => condition(t));
        }
    }
}