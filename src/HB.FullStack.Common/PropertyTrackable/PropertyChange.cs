using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// One change of Property
    /// </summary>
    public class PropertyChange
    {
        /// <summary>
        /// 改变的属性名
        /// </summary>
        public string PropertyName { get; set; } = null!;

        public JsonElement OldValue { get; set; }

        public JsonElement NewValue { get; set; }

        [JsonConstructor]
        public PropertyChange(string name, object? oldValue, object? newValue)
        {
            PropertyName = name;

            OldValue = SerializeUtil.ToJsonElement(oldValue);
            NewValue = SerializeUtil.ToJsonElement(newValue);
        }

        public PropertyChange(PropertyChange other)
        {
            PropertyName = other.PropertyName;
            OldValue = other.OldValue;
            NewValue = other.NewValue;
        }
    }
}