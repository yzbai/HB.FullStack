using System.Collections.Generic;

namespace HB.FullStack.Common.PropertyTrackable
{
    public interface IPropertyTrackableObject
    {
        void StartTrack();

        void StopTrack();

        void Clear();

        IList<PropertyChange> GetPropertyChanges(bool mergeMultipleChanges = true);

        void Track<T>(string propertyName, T oldValue, T newValue);

        void TrackOldValue<T>(string propertyName, T oldValue);

        void TrackNewValue<T>(string propertyName, T newValue);
    }
}