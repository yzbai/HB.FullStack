using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

namespace HB.FullStack.Common.PropertyTrackable
{
    public class PropertyChangePack : ValidatableObject
    {
        public IDictionary<PropertyName, PropertyChange> PropertyChanges { get; set; } = new Dictionary<PropertyName, PropertyChange>();

        public IDictionary<PropertyName, JsonElement> AddtionalProperties { get; set; } = new Dictionary<PropertyName, JsonElement>();

        public void AddAddtionalProperty(PropertyName propertyName, object? propertyValue)
        {
            AddtionalProperties[propertyName] = SerializeUtil.ToJsonElement(propertyValue);
        }
    }

    public static class PropertyChangePackExtensions
    {
        public static bool ContainsProperty(this PropertyChangePack cp, PropertyName propertyName)
        {
            return cp.PropertyChanges.ContainsKey(propertyName);
        }

        public static bool ContainsAddtionalProperty(this PropertyChangePack cp, string propertyName)
        {
            return cp.AddtionalProperties.ContainsKey(propertyName);
        }
    }
}