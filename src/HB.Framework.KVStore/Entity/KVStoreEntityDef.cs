﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace HB.Framework.KVStore.Entity
{
    public class KVStoreEntityDef
    {
        public string KVStoreName { get; set; }

        public Type EntityType { get; set; }

        public string EntityFullName { get; set; }

        public IDictionary<int, PropertyInfo> KeyPropertyInfos { get; } = new Dictionary<int, PropertyInfo>();

        //public string KeyPropertyName
        //{
        //    get
        //    {
        //        if (KeyPropertyInfo == null)
        //        {
        //            return null;
        //        }

        //        return KeyPropertyInfo.Name;
        //    }
        //}

        //public Type KeyPropertyType
        //{
        //    get
        //    {
        //        if (KeyPropertyInfo == null)
        //        {
        //            return null;
        //        }

        //        return KeyPropertyInfo.PropertyType;
        //    }
        //}

        public KVStoreEntityDef() { }        
    }
}
