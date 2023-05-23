﻿using System;
using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Meta;

namespace HB.FullStack.Common.PropertyTrackable
{
    public static class IPropertyTrackableObjectExtensions
    {
        /// <summary>
        /// This will change the Timestamp if the trackableObject is ITimestamp
        /// </summary>
        /// <param name="trackableObject"></param>
        /// <returns></returns>
        public static PropertyChangePack GetPropertyChangePack(this IPropertyTrackableObject trackableObject)
        {
            //TODO: 需要考虑锁吗?

            var addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(trackableObject);
            var propertyChanges = GetPropertyChangeDict(trackableObject);

            if (trackableObject is ITimestamp timestampModel && !propertyChanges.ContainsKey(nameof(ITimestamp.Timestamp)))
            {
                long curTimestamp = TimeUtil.Timestamp;
                
                propertyChanges[nameof(ITimestamp.Timestamp)] = new PropertyChange(nameof(ITimestamp.Timestamp), timestampModel.Timestamp, curTimestamp);
                
                timestampModel.Timestamp = curTimestamp;
            }


            var changePack = new PropertyChangePack
            {
                PropertyChanges = propertyChanges,
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.Name, pv => SerializeUtil.ToJsonElement(pv.Value))
            };

            return changePack;

            static IDictionary<PropertyName, PropertyChange> GetPropertyChangeDict(IPropertyTrackableObject trackableObject)
            {
                Dictionary<PropertyName, PropertyChange>? propertyChangsDict = new Dictionary<string, PropertyChange>();

                foreach (PropertyChange curProperty in trackableObject.GetChanges())
                {
                    if (propertyChangsDict.TryGetValue(curProperty.PropertyName, out PropertyChange? storedProperty))
                    {
                        storedProperty.NewValue = curProperty.NewValue;
                    }
                    else
                    {
                        propertyChangsDict.Add(curProperty.PropertyName, new PropertyChange(curProperty));
                    }
                }

                return propertyChangsDict;
            }
        }
    }
}