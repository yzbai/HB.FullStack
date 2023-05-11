using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HB.FullStack.Common.PropertyTrackable
{
    /// <summary>
    /// One change of Property
    /// 使用序列化固定值
    /// 选择Json的原因是是为了在两端传输方便。但的确只在一端操作时，Json序列有性能考虑之忧
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
        public PropertyChange() { }

        public PropertyChange(string propertyName, object? oldValue, object? newValue)
        {
            PropertyName = propertyName;

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