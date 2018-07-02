using System;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    public class KVStoreEntityDef
    {
        public string KVStoreName { get; set; }

        public int KVStoreIndex { get; set; }

        public Type EntityType { get; set; }

        public string EntityFullName { get; set; }

        public PropertyInfo KeyPropertyInfo { get; set; }

        public string KeyPropertyName
        {
            get
            {
                if (KeyPropertyInfo == null)
                {
                    return null;
                }

                return KeyPropertyInfo.Name;
            }
        }

        public Type KeyPropertyType
        {
            get
            {
                if (KeyPropertyInfo == null)
                {
                    return null;
                }

                return KeyPropertyInfo.PropertyType;
            }
        }

        public KVStoreEntityDef() { }        
    }
}
