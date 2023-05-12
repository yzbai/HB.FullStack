using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace HB.FullStack.Common.PropertyTrackable
{
    public class PropertyChangePack : ValidatableObject
    {
        public IList<PropertyChange> PropertyChanges { get; set; } = new List<PropertyChange>();

        public IDictionary<PropertyName, JsonElement> AddtionalProperties { get; set; } = new Dictionary<PropertyName, JsonElement>();

        public void AddAddtionalProperty(PropertyName propertyName, object? propertyValue)
        {
            AddtionalProperties[propertyName] = SerializeUtil.ToJsonElement(propertyValue);
        }
    }

    public static class PropertyChangePackExtensions
    {
        public static bool ContainsProperty(this PropertyChangePack cp, string propertyName)
        {
            return cp.PropertyChanges.FirstOrDefault(change => change.PropertyName == propertyName) != null;
        }

        public static bool ContainsAddtionalProperty(this PropertyChangePack cp, string propertyName)
        {
            return cp.AddtionalProperties.ContainsKey(propertyName);
        }
    }
}