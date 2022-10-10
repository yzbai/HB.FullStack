using System;
using System.Collections.Generic;
using System.Linq;

using HB.FullStack.Common.Meta;

namespace HB.FullStack.Common.PropertyTrackable
{
    public interface IPropertyTrackableObject
    {
        void StartTrack();

        void EndTrack();

        void Clear();

        IList<ChangedProperty> GetChangedProperties(bool mergeMultipleChanged = true);

        void Track<T>(string propertyName, T oldValue, T newValue);

        void TrackNewValue<T>(string propertyName, string? propertyPropertyName, T newValue);

        void TrackOldValue<T>(string propertyName, string? propertyPropertyName, T oldValue);
    }

    public static class IPropertyTrackableObjectExtensions
    {
        public static ChangedPack GetChangedPack(this IPropertyTrackableObject model, object id)
        {
            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(model);

            ChangedPack changedPack = new ChangedPack
            {
                Id = id,
                ChangedProperties = model.GetChangedProperties(),
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => SerializeUtil.ToJsonElement(pv.Value))
            };

            return changedPack;
        }
    }
}