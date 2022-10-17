using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Common.Api
{
    public class ChangedPackDto : ValidatableObject
    {
        /// <summary>
        /// ModelId
        /// </summary>
        [Required]
        public object? Id { get; set; }

        public IList<ChangedPropertyDto> ChangedProperties { get; set; } = new List<ChangedPropertyDto>();

        /// <summary>
        /// 要求所有的ForeignKey 都放在这里
        /// </summary>
        public IDictionary<string, JsonElement> AddtionalProperties { get; set; } = new Dictionary<string, JsonElement>();

        public void AddAddtionalProperty(string propertyName, object? propertyValue)
        {
            AddtionalProperties[propertyName] = SerializeUtil.ToJsonElement(propertyValue);
        }
    }

    public static class ChangedPackExtension
    {
        public static ChangedPackDto ToDto(this ChangedPack changedPack)
        {
            return new ChangedPackDto
            {
                Id = changedPack.Id,

                ChangedProperties = changedPack.ChangedProperties
                .Select(c => c.ToDto())
                .ToList(),

                AddtionalProperties = changedPack.AddtionalProperties
                    .Select(kv => new KeyValuePair<string, JsonElement>(kv.Key, SerializeUtil.ToJsonElement(kv.Value)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }
    }
}