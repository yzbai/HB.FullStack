using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HB.FullStack.Common.PropertyTrackable
{
    public class PropertyTrackableObject : IPropertyTrackableObject
    {
        private bool _startTrack;

        private readonly List<ChangedProperty> _changedProperties = new List<ChangedProperty>();

        private readonly Dictionary<string, ChangedProperty> _updatingProperties = new Dictionary<string, ChangedProperty>();

        public void StartTrack()
        {
            _startTrack = true;
        }

        public void EndTrack()
        {
            _startTrack = false;
        }

        public void Track<T>(string propertyName, T oldValue, T newValue)
        {
            if (!_startTrack)
            {
                return;
            }

            _changedProperties.Add(new ChangedProperty(propertyName, oldValue, newValue));
        }

        public void TrackOldValue<T>(string propertyName, string? propertyPropertyName, T oldValue)
        {
            if (!_startTrack)
            {
                return;
            }

            string key = propertyName + propertyPropertyName;

            if (_updatingProperties.ContainsKey(key))
            {
                throw new InvalidOperationException("ITrackPropertyChangedReentrancyNotAllowed");
            }

            _updatingProperties[key] = new ChangedProperty(propertyName, oldValue, null, propertyPropertyName);
        }

        public void TrackNewValue<T>(string propertyName, string? propertyPropertyName, T newValue)
        {
            if (!_startTrack)
            {
                return;
            }

            string key = propertyName + propertyPropertyName;

            if (!_updatingProperties.TryGetValue(key, out ChangedProperty? updatingProperty))
            {
                throw new InvalidOperationException("ITrackPropertyChangedNoOldValueTracked");
            }

            updatingProperty.NewValue = newValue;

            _updatingProperties.Remove(key);

            _changedProperties.Add(updatingProperty);
        }

        public void Clear()
        {
            _changedProperties.Clear();
        }

        public IList<ChangedProperty> GetChangedProperties(bool mergeMultipleChanged = true)
        {
            //TODO: 需要考虑锁吗?

            if (!mergeMultipleChanged)
            {
                return _changedProperties;
            }

            Dictionary<string, ChangedProperty> dict = new Dictionary<string, ChangedProperty>();

            foreach (ChangedProperty curProperty in _changedProperties)
            {
                if (dict.TryGetValue(curProperty.PropertyName, out ChangedProperty? storedProperty))
                {
                    storedProperty.NewValue = curProperty.NewValue;
                }
                else
                {
                    dict.Add(curProperty.PropertyName, curProperty);
                }
            }

            return Enumerable.ToList(dict.Values);
        }

        protected void SetAndTrackProperty<T>([NotNullIfNotNull("newValue")] ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
            {
                return;
            }

            Track(propertyName!, field, newValue);

            field = newValue;
        }
    }
}