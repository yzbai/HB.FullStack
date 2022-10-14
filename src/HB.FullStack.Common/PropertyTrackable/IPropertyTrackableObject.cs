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

        IList<ChangedProperty2> GetChangedProperties(bool mergeMultipleChanged = true);

        void Track<T>(string propertyName, T oldValue, T newValue);

        void TrackNewValue<T>(string propertyName, string? propertyPropertyName, T newValue);

        void TrackOldValue<T>(string propertyName, string? propertyPropertyName, T oldValue);
    }

    public static class IPropertyTrackableObjectExtensions
    {
        public static ChangedPack2 GetChangedPack(this IPropertyTrackableObject model, object id)
        {
            PropertyValue[] addtionalProperties = MetaAccess.GetPropertyValuesByAttribute<AddtionalPropertyAttribute>(model);

            ChangedPack2 changedPack = new ChangedPack2
            {
                Id = id,
                ChangedProperties = model.GetChangedProperties(),
                AddtionalProperties = addtionalProperties.ToDictionary(pv => pv.PropertyName, pv => pv.Value)
            };

            return changedPack;
        }
    }
}