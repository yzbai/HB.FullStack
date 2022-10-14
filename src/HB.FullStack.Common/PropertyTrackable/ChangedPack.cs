using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace HB.FullStack.Common.PropertyTrackable
{

    public class ChangedPack2 : ValidatableObject
    {
        /// <summary>
        /// ModelId
        /// </summary>
        [Required]
        public object? Id { get; set; }

        public IList<ChangedProperty2> ChangedProperties { get; set; } = new List<ChangedProperty2>();

        /// <summary>
        /// 要求所有的ForeignKey 都放在这里
        /// </summary>
        public IDictionary<string, object?> AddtionalProperties { get; set; } = new Dictionary<string, object?>();

        public void AddAddtionalProperty(string propertyName, object? propertyValue)
        {
            AddtionalProperties[propertyName] = propertyValue;
        }
    }
}