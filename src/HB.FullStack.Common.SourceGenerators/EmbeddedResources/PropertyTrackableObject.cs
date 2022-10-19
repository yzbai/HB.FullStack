
public class PropertyTrackableObject : global::HB.FullStack.Common.PropertyTrackable.IPropertyTrackableObject
{
    public bool _startTrack = false;

    private readonly global::System.Collections.Generic.List<global::HB.FullStack.Common.PropertyTrackable.ChangedProperty> _changedProperties = new global::System.Collections.Generic.List<global::HB.FullStack.Common.PropertyTrackable.ChangedProperty>();

    private readonly global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.ChangedProperty> _updatingProperties = new global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.ChangedProperty>();

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

        _changedProperties.Add(new global::HB.FullStack.Common.PropertyTrackable.ChangedProperty(propertyName, oldValue, newValue));
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
            throw new global::System.InvalidOperationException("ITrackPropertyChangedReentrancyNotAllowed");
        }

        _updatingProperties[key] = new global::HB.FullStack.Common.PropertyTrackable.ChangedProperty(propertyName, oldValue, null, propertyPropertyName);
    }

    public void TrackNewValue<T>(string propertyName, string? propertyPropertyName, T newValue)
    {
        if (!_startTrack)
        {
            return;
        }

        string key = propertyName + propertyPropertyName;

        if (!_updatingProperties.TryGetValue(key, out global::HB.FullStack.Common.PropertyTrackable.ChangedProperty? updatingProperty))
        {
            throw new global::System.InvalidOperationException("ITrackPropertyChangedNoOldValueTracked");
        }

        updatingProperty.NewValue = newValue;

        _updatingProperties.Remove(key);

        _changedProperties.Add(updatingProperty);
    }

    public void Clear()
    {
        _changedProperties.Clear();
    }

    public global::System.Collections.Generic.IList<global::HB.FullStack.Common.PropertyTrackable.ChangedProperty> GetChangedProperties(bool mergeMultipleChanged = true)
    {
        //TODO: 需要考虑锁吗?

        if (!mergeMultipleChanged)
        {
            return _changedProperties;
        }

        global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.ChangedProperty> dict = new global::System.Collections.Generic.Dictionary<string, global::HB.FullStack.Common.PropertyTrackable.ChangedProperty>();

        foreach (global::HB.FullStack.Common.PropertyTrackable.ChangedProperty curProperty in _changedProperties)
        {
            if (dict.TryGetValue(curProperty.PropertyName, out global::HB.FullStack.Common.PropertyTrackable.ChangedProperty? storedProperty))
            {
                storedProperty.NewValue = curProperty.NewValue;
            }
            else
            {
                dict.Add(curProperty.PropertyName, curProperty);
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