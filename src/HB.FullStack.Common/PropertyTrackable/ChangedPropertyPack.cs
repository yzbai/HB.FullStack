using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HB.FullStack.Common.PropertyTrackable
{
    public class ChangedPack
    {
        /// <summary>
        /// ModelId
        /// </summary>
        [Required]
        public object? Id { get; set; }

        public IList<ChangedProperty> ChangedProperties { get; set; } = new List<ChangedProperty>();

        /// <summary>
        /// 要求所有的ForeignKey 都放在这里
        /// </summary>
        public IDictionary<string, JsonElement> AddtionalProperties { get; set; } = new Dictionary<string, JsonElement>();

        public void AddAddtionalProperty(string propertyName, object? propertyValue)
        {
            AddtionalProperties[propertyName] = SerializeUtil.ToJsonElement(propertyValue);
        }
    }
}