using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.PropertyTrackable
{
    public class ChangedProperty
    {
        /// <summary>
        /// 改变的属性名
        /// </summary>
        public string PropertyName { get; set; } = null!;

        /// <summary>
        /// 改变的属性内的属性名
        /// </summary>
        public string? PropertyPropertyName { get; set; }

        public JsonElement OldValue { get; set; }

        public JsonElement NewValue { get; set; }

        [JsonConstructor]
        public ChangedProperty(string name, object? oldValue, object? newValue, string? propertyPropertyName = null)
        {
            PropertyName = name;
            PropertyPropertyName = propertyPropertyName;

            OldValue = SerializeUtil.ToJsonElement(oldValue);
            NewValue = SerializeUtil.ToJsonElement(newValue);
        }
    }
}