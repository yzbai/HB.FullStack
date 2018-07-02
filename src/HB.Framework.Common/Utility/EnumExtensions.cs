
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace System
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var typeInfo = enumVal.GetType().GetTypeInfo();
            var memInfos = typeInfo.GetMember(enumVal.ToString());

            if (memInfos == null || memInfos.Length == 0)
            {
                return null;
            }

            var attributes = memInfos[0].GetCustomAttributes(typeof(T), false);

            if (attributes == null || attributes.Count() == 0)
            {
                return null;
            }

            return (T)attributes.ElementAt(0);
        }

        public static string GetDescription(this Enum enumVal)
        {
            DescriptionAttribute attr = enumVal.GetAttributeOfType<DescriptionAttribute>();

            if (attr == null)
            {
                return Enum.GetName(enumVal.GetType(), enumVal);
            }

            return attr.Description;
        }
    }
}
