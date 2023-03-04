
/*
 * Copy of HB.FullStack.Common.PropertyTrackable.PropertyTrackableObject
 */
/// <summary>
/// Use [PropertyTrackableObjectAttribute] to put these memebers into your class
/// Keep full namespace before every type, because of source generation has exactly copy of this.
/// When modify this class, update source generation embeded source.
/// </summary>
public class PropertyTrackableObject : global::HB.FullStack.Common.PropertyTrackable.IPropertyTrackableObject
{
    public bool _startTrack = false;

    public global::System.Collections.Generic.IList<global::HB.FullStack.Common.PropertyTrackable.PropertyChange> Changes { get; private set; } = new global::System.Collections.Generic.List<global::HB.FullStack.Common.PropertyTrackable.PropertyChange>();

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
        Changes.Clear();
    }

    public void Track<T>(string propertyName, T oldValue, T newValue)
    {
        if (!_startTrack)
        {
            return;
        }

        Changes.Add(new global::HB.FullStack.Common.PropertyTrackable.PropertyChange(propertyName, oldValue, newValue));
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

        Changes.Add(updatingProperty);
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

    public global::HB.FullStack.Common.PropertyTrackable.PropertyChangePack GetPropertyChanges(bool mergeMultipleChanged = true)
    {
        return global::HB.FullStack.Common.PropertyTrackable.PropertyTrackableObjectStatic.GetPropertyChanges(this, mergeMultipleChanged);
    }
}