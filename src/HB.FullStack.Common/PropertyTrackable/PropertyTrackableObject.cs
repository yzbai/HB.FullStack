using System;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// Use [PropertyTrackableObjectAttribute] to put these memebers into your class
    /// Keep full namespace before every type, because of source generation has exactly copy of this.
    /// When modify this class, update source generation embeded source.
    /// </summary>
    public class PropertyTrackableObject : global::HB.FullStack.Common.PropertyTrackable.IPropertyTrackableObject
    {
        public bool _startTrack = false;

        private readonly global::System.Collections.Generic.List<global::HB.FullStack.Common.PropertyTrackable.PropertyChange> _changedProperties = new global::System.Collections.Generic.List<global::HB.FullStack.Common.PropertyTrackable.PropertyChange>();

        private readonly global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.PropertyChange> _updatingProperties = new global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.PropertyChange>();

        public void StartTrack()
        {
            _startTrack = true;
        }

        public void StopTrack()
        {
            _startTrack = false;
        }

        public void Clear()
        {
            _changedProperties.Clear();
        }

        public void Track<T>(string propertyName, T oldValue, T newValue)
        {
            if (!_startTrack)
            {
                return;
            }

            _changedProperties.Add(new global::HB.FullStack.Common.PropertyTrackable.PropertyChange(propertyName, oldValue, newValue));
        }

        public void TrackOldValue<T>(string propertyName, T oldValue)
        {
            if (!_startTrack)
            {
                return;
            }

            if (_updatingProperties.ContainsKey(propertyName))
            {
                throw new global::System.InvalidOperationException("ITrackPropertyChangedReentrancyNotAllowed");
            }

            _updatingProperties[propertyName] = new global::HB.FullStack.Common.PropertyTrackable.PropertyChange(propertyName, oldValue, null);
        }

        public void TrackNewValue<T>(string propertyName, T newValue)
        {
            if (!_startTrack)
            {
                return;
            }

            if (!_updatingProperties.TryGetValue(propertyName, out global::HB.FullStack.Common.PropertyTrackable.PropertyChange? updatingProperty))
            {
                throw new global::System.InvalidOperationException("ITrackPropertyChangedNoOldValueTracked");
            }

            updatingProperty.NewValue = global::System.SerializeUtil.ToJsonElement(newValue);

            _updatingProperties.Remove(propertyName);

            _changedProperties.Add(updatingProperty);
        }

        public global::System.Collections.Generic.IList<global::HB.FullStack.Common.PropertyTrackable.PropertyChange> GetPropertyChanges(bool mergeMultipleChanged = true)
        {
            //TODO: 需要考虑锁吗?

            if (!mergeMultipleChanged)
            {
                return _changedProperties;
            }

            global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.PropertyChange> dict = new global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.PropertyChange>();

            foreach (global::HB.FullStack.Common.PropertyTrackable.PropertyChange curProperty in _changedProperties)
            {
                if (dict.TryGetValue(curProperty.PropertyName, out global::HB.FullStack.Common.PropertyTrackable.PropertyChange? storedProperty))
                {
                    storedProperty.NewValue = curProperty.NewValue;
                }
                else
                {
                    dict.Add(curProperty.PropertyName, new global::HB.FullStack.Common.PropertyTrackable.PropertyChange(curProperty));
                }
            }

            return global::System.Linq.Enumerable.ToList(dict.Values);
        }

        protected void SetAndTrackProperty<T>([global::System.Diagnostics.CodeAnalysis.NotNullIfNotNull("newValue")] ref T field, T newValue, [global::System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            if (global::System.Collections.Generic.EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return;
            }

            Track(propertyName!, field, newValue);

            field = newValue;
        }
    }
}