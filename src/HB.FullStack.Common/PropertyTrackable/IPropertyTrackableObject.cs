using System.Collections.Generic;

namespace HB.FullStack.Common.PropertyTrackable
{
    public interface IPropertyTrackableObject
    {
        //IList<PropertyChange> _changes { get; }

        IList<PropertyChange> GetChanges();

        //TODO: 是否需要自动StartTrack，比如刚从Db拿到手
        void StartTrack();

        void StopTrack();

        //use method but not property
        bool IsTracking();

        void Clear();

        void Track<T>(string propertyName, T oldValue, T newValue);

        void TrackOldValue<T>(string propertyName, T oldValue);

        void TrackNewValue<T>(string propertyName, T newValue);
    }
}