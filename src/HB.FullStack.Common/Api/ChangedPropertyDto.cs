using System;
using System.Text.Json;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Common.Api
{
    public class ChangedPropertyDto
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

        [OnlyForJsonConstructor]
        public ChangedPropertyDto(string name, object? oldValue, object? newValue, string? propertyPropertyName = null)
        {
            PropertyName = name;
            PropertyPropertyName = propertyPropertyName;

            OldValue = SerializeUtil.ToJsonElement(oldValue);
            NewValue = SerializeUtil.ToJsonElement(newValue);
        }
    }

    public static class ChangedPropertyExtension
    {
        public static ChangedPropertyDto ToDto(this ChangedProperty2 changedProperty)
        {
            return new ChangedPropertyDto(changedProperty.PropertyName, changedProperty.OldValue, changedProperty.NewValue, changedProperty.PropertyPropertyName);
        }
    }
}