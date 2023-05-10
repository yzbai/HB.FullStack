using System;
using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Meta;

namespace HB.FullStack.Common.PropertyTrackable
{
    public static class PropertyTrackableObjectStatic
    {
        public static PropertyChangePack GetPropertyChanges(IPropertyTrackableObject trackableObject, bool mergeMultipleChanged = true)
        {
            //TODO: 需要考虑锁吗?

            //if (trackableObject is ITimestampModel timestampModel)
            //{
            //    timestampModel.Timestamp = TimeUtil.Timestamp;
            //}

            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(trackableObject);
            IList<PropertyChange>? propertyChanges = GetPropertyChangeList(trackableObject, mergeMultipleChanged);

            PropertyChangePack changePack = new PropertyChangePack
            {
                PropertyChanges = propertyChanges,
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => SerializeUtil.ToJsonElement(pv.Value))
            };

            return changePack;

            static IList<PropertyChange> GetPropertyChangeList(IPropertyTrackableObject trackableObject, bool mergeMultipleChanged)
            {
                IList<PropertyChange>? propertyChanges = null;

                if (mergeMultipleChanged)
                {
                    Dictionary<string, PropertyChange> dict = new Dictionary<string, PropertyChange>();

                    foreach (PropertyChange curProperty in trackableObject.GetChanges())
                    {
                        if (dict.TryGetValue(curProperty.PropertyName, out PropertyChange? storedProperty))
                        {
                            storedProperty.NewValue = curProperty.NewValue;
                        }
                        else
                        {
                            dict.Add(curProperty.PropertyName, new PropertyChange(curProperty));
                        }
                    }

                    propertyChanges = Enumerable.ToList(dict.Values);
                }
                else
                {
                    propertyChanges = new List<PropertyChange>();

                    foreach (PropertyChange curProperty in trackableObject.GetChanges())
                    {
                        propertyChanges.Add(new PropertyChange(curProperty));
                    }
                }

                //propertyChanges.OrderBy(pc => pc.PropertyName);

                return propertyChanges;
            }
        }
    }
}