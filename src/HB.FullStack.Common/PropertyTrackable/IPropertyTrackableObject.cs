using System.Collections.Generic;

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
}